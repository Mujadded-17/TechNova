// Startup Dashboard - Facebook Style
// Handles posts, photos, videos, and feed interactions

(function() {
	'use strict';

	// Configuration
	const config = {
		endpoints: {
			createPost: '/Startup/CreatePost',
			editPost: '/Startup/EditPost',
			deletePost: '/Startup/DeletePost',
			uploadPhoto: '/Startup/UploadPhoto',
			editPhoto: '/Startup/EditPhoto',
			deletePhoto: '/Startup/DeletePhoto',
			uploadVideo: '/Startup/UploadVideo',
			editVideo: '/Startup/EditVideo',
			deleteVideo: '/Startup/DeleteVideo',
			getFeed: '/Startup/GetFeed'
		}
	};

	// State
	const state = {
		posts: [],
		pendingPhotos: [],
		editingPostId: null
	};

	// DOM Elements
	const elements = {
		postBtn: document.getElementById('postBtn'),
		postContentInput: document.getElementById('postContentInput'),
		postPhotoBtn: document.getElementById('postPhotoBtn'),
		photoFileInput: document.getElementById('photoFileInput'),
		postModal: document.getElementById('postModal'),
		postModalContent: document.getElementById('postModalContent'),
		closePostModal: document.getElementById('closePostModal'),
		cancelPostModal: document.getElementById('cancelPostModal'),
		submitPost: document.getElementById('submitPost'),
		postsFeed: document.getElementById('postsFeed'),
		openCreatePost: document.getElementById('openCreatePost'),
		photoPreview: document.getElementById('photoPreview'),
		photoPreviewContainer: document.getElementById('photoPreviewContainer'),
		photoGallery: document.getElementById('photoGallery'),
		photoGallerySection: document.getElementById('photoGallerySection')
	};

	// Rendered by the view; a static script cannot read the Razor model.
	const companyName = elements.postsFeed.dataset.company || '';
	const logoPath = elements.postsFeed.dataset.logo || '';

	// Initialize
	function init() {
		attachEventListeners();
		loadFeed();
	}

	// Event Listeners
	function attachEventListeners() {
		// The inline box is a shortcut: whatever was typed there is carried
		// into the modal rather than silently dropped.
		elements.postBtn.addEventListener('click', () => openPostModal(elements.postContentInput.value));
		elements.postContentInput.addEventListener('keydown', (e) => {
			if (e.key === 'Enter') { e.preventDefault(); openPostModal(elements.postContentInput.value); }
		});
		elements.openCreatePost.addEventListener('click', () => openPostModal());
		elements.postPhotoBtn.addEventListener('click', () => {
			openPostModal(elements.postContentInput.value);
			elements.photoFileInput.click();
		});
		elements.photoFileInput.addEventListener('change', handlePhotoSelection);
		elements.closePostModal.addEventListener('click', closePostModal);
		elements.cancelPostModal.addEventListener('click', closePostModal);
		elements.submitPost.addEventListener('click', submitPost);
		elements.postModal.addEventListener('click', (e) => {
			if (e.target === elements.postModal) closePostModal();
		});

		// Keyboard shortcuts
		document.addEventListener('keydown', (e) => {
			if (e.key === 'Escape' && elements.postModal.classList.contains('active')) {
				closePostModal();
			}
		});
	}

	// Post Modal Functions
	function openPostModal(prefill) {
		if (typeof prefill === 'string' && prefill.trim() && !state.editingPostId) {
			elements.postModalContent.value = prefill;
		}
		elements.postModal.classList.add('active');
		elements.postModal.setAttribute('aria-hidden', 'false');
		elements.postModalContent.focus();
	}

	function closePostModal() {
		elements.postModal.classList.remove('active');
		elements.postModal.setAttribute('aria-hidden', 'true');
		elements.postModalContent.value = '';
		elements.postContentInput.value = '';
		state.editingPostId = null;
		elements.submitPost.textContent = 'Post';
		document.querySelector('#postModal .edit-modal-header h3').textContent = 'Create post';
		clearPendingPhotos();
	}

	// Photo/Video Selection
	function handlePhotoSelection(e) {
		const files = Array.from(e.target.files);
		files.forEach(file => {
			if (!file.type.startsWith('image/')) {
				showNotification(`${file.name} is not an image.`, 'error');
				return;
			}
			if (file.size > 10 * 1024 * 1024) {
				showNotification(`${file.name} is larger than 10 MB.`, 'error');
				return;
			}
			const reader = new FileReader();
			reader.onload = (event) => {
				state.pendingPhotos.push({
					file: file,
					preview: event.target.result
				});
				updatePhotoPreview();
			};
			reader.readAsDataURL(file);
		});
		e.target.value = '';
	}

	function updatePhotoPreview() {
		if (state.pendingPhotos.length === 0) {
			elements.photoPreviewContainer.style.display = 'none';
			elements.photoPreview.innerHTML = '';
			return;
		}

		elements.photoPreviewContainer.style.display = 'block';
		elements.photoPreview.innerHTML = state.pendingPhotos.map((photo, index) => `
			<div style="position: relative; border-radius: 8px; overflow: hidden;">
				<img src="${photo.preview}" alt="Preview" style="width: 100%; aspect-ratio: 1; object-fit: cover;" />
				<button type="button" class="remove-photo-btn" data-index="${index}" 
						style="position: absolute; top: 5px; right: 5px; background: rgba(0,0,0,0.7); color: white; 
							   border: none; border-radius: 50%; width: 24px; height: 24px; cursor: pointer; font-size: 16px;">
					×
				</button>
			</div>
		`).join('');

		// Attach remove handlers
		document.querySelectorAll('.remove-photo-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const index = parseInt(btn.dataset.index);
				state.pendingPhotos.splice(index, 1);
				updatePhotoPreview();
			});
		});
	}

	function clearPendingPhotos() {
		state.pendingPhotos = [];
		updatePhotoPreview();
	}

	// Submit Post
	async function submitPost() {
		const content = elements.postModalContent.value.trim();

		if (!content) {
			showNotification('Write something before posting.', 'error');
			elements.postModalContent.focus();
			return;
		}

		if (content.length > 5000) {
			showNotification('Posts are limited to 5,000 characters.', 'error');
			return;
		}

		const editing = state.editingPostId;

		try {
			elements.submitPost.disabled = true;
			elements.submitPost.textContent = editing ? 'Saving...' : 'Posting...';

			let postId = editing;

			if (editing) {
				const editResponse = await fetch(config.endpoints.editPost, {
					method: 'POST',
					headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
					body: `postId=${encodeURIComponent(editing)}&content=${encodeURIComponent(content)}&__RequestVerificationToken=${encodeURIComponent(getCSRFToken())}`
				});

				if (!editResponse.ok) {
					throw new Error('edit');
				}
			} else {
				const createResponse = await fetch(config.endpoints.createPost, {
					method: 'POST',
					headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
					body: `content=${encodeURIComponent(content)}&__RequestVerificationToken=${encodeURIComponent(getCSRFToken())}`
				});

				if (!createResponse.ok) {
					throw new Error('create');
				}

				const postData = await createResponse.json();
				postId = postData.postId;
			}

			// Upload attached photos
			if (state.pendingPhotos.length > 0) {
				for (const photo of state.pendingPhotos) {
					const formData = new FormData();
					formData.append('file', photo.file);
					formData.append('caption', '');
					formData.append('postId', postId);
					formData.append('__RequestVerificationToken', getCSRFToken());

					const uploadResponse = await fetch(config.endpoints.uploadPhoto, {
						method: 'POST',
						body: formData
					});

					if (!uploadResponse.ok) {
						const reason = await uploadResponse.text().catch(() => '');
						showNotification(reason || 'One of the photos could not be uploaded.', 'error');
					}
				}
			}

			closePostModal();
			loadFeed();
			showNotification(editing ? 'Post updated.' : 'Post published.', 'success');
		} catch (error) {
			showNotification(editing ? 'Could not save your changes. Please try again.' : 'Could not publish the post. Please try again.', 'error');
		} finally {
			elements.submitPost.disabled = false;
			if (!state.editingPostId) elements.submitPost.textContent = 'Post';
		}
	}

	// Load Feed
	async function loadFeed() {
		try {
			const response = await fetch(config.endpoints.getFeed);
			if (!response.ok) {
				throw new Error('Failed to load feed');
			}

			const posts = await response.json();
			state.posts = posts;
			renderFeed(posts);
			renderPhotos(posts);
		} catch (error) {
			elements.postsFeed.innerHTML = `
				<div class="empty-state">
					<div class="empty-state-title">Couldn't load your updates</div>
					<div class="empty-state-text">Check your connection and <a href="#" data-retry-feed>try again</a>.</div>
				</div>
			`;
			const retry = elements.postsFeed.querySelector('[data-retry-feed]');
			if (retry) retry.addEventListener('click', (e) => { e.preventDefault(); loadFeed(); });
		}
	}

	// Render Feed
	function renderFeed(posts) {
		if (!posts || posts.length === 0) {
			elements.postsFeed.innerHTML = `
				<div class="empty-state">
					<div class="empty-state-title">No posts yet</div>
					<div class="empty-state-text">Share your first startup update to get started</div>
				</div>
			`;
			return;
		}

		elements.postsFeed.innerHTML = posts.map(post => renderPostCard(post)).join('');

		// Attach event listeners to post cards
		attachPostCardListeners();
	}

	function renderPostCard(post) {
		const date = new Date(post.createdAt);
		const timeAgo = getTimeAgo(date);
		const photos = post.photos || [];

		const edited = post.updatedAt ? ' · edited' : '';
		const avatar = logoPath
			? `<img class="post-avatar" src="${escapeHtml(logoPath)}" alt="" />`
			: `<div class="post-avatar" aria-hidden="true"></div>`;

		return `
			<article class="feed-post fade-in" data-post-id="${post.postID}">
				<div class="post-header">
					<div class="post-author-info">
						${avatar}
						<div>
							<h4>${escapeHtml(companyName)}</h4>
							<p class="post-time">${timeAgo}${edited}</p>
						</div>
					</div>
				</div>
				<div class="post-content">${escapeHtml(post.content)}</div>
				${photos.length > 0 ? `
					<div class="post-media">
						${photos.map(photo => `
							<img src="${escapeHtml(photo.filePath)}" alt="${escapeHtml(photo.caption || 'Post photo')}" loading="lazy" />
						`).join('')}
					</div>
				` : ''}
				<div class="post-footer">
					<button type="button" class="edit-post-btn" data-post-id="${post.postID}">Edit</button>
					<button type="button" class="delete-post-btn" data-post-id="${post.postID}">Delete</button>
				</div>
			</article>
		`;
	}

	function attachPostCardListeners() {
		document.querySelectorAll('.edit-post-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const postId = btn.dataset.postId;
				const post = state.posts.find(p => p.postID.toString() === postId);
				if (post) {
					editPost(post);
				}
			});
		});

		document.querySelectorAll('.delete-post-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const postId = btn.dataset.postId;
				if (confirm('Are you sure you want to delete this post?')) {
					deletePost(postId);
				}
			});
		});
	}

	// Edit Post
	function editPost(post) {
		state.editingPostId = post.postID;
		elements.postModalContent.value = post.content;
		elements.submitPost.textContent = 'Save changes';
		document.querySelector('#postModal .edit-modal-header h3').textContent = 'Edit post';
		openPostModal();
	}

	// Delete Post
	async function deletePost(postId) {
		try {
			const response = await fetch(config.endpoints.deletePost, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/x-www-form-urlencoded'
				},
				body: `postId=${postId}&__RequestVerificationToken=${encodeURIComponent(getCSRFToken())}`
			});

			if (!response.ok) {
				throw new Error('Failed to delete post');
			}

			loadFeed();
			showNotification('Post deleted.', 'success');
		} catch (error) {
			showNotification('Could not delete the post. Please try again.', 'error');
		}
	}

	// Render the photo gallery from the posts already loaded — no second request.
	function renderPhotos(posts) {
		try {
			let allPhotos = [];

			// Collect all photos from posts
			posts.forEach(post => {
				if (post.photos && post.photos.length > 0) {
					allPhotos = allPhotos.concat(post.photos);
				}
			});

			if (allPhotos.length === 0) {
				elements.photoGallerySection.style.display = 'none';
				return;
			}

			elements.photoGallerySection.style.display = 'block';
			elements.photoGallery.innerHTML = allPhotos.map(photo => `
				<div class="gallery-item" data-photo-id="${photo.photoID}">
					<img src="${escapeHtml(photo.filePath)}" alt="${escapeHtml(photo.caption || 'Gallery photo')}" loading="lazy" />
					<div class="gallery-item-overlay">
						<button type="button" class="delete-photo-btn" data-photo-id="${photo.photoID}" title="Delete photo" aria-label="Delete photo">Delete</button>
					</div>
				</div>
			`).join('');

			attachPhotoEventListeners();
		} catch (error) {
			elements.photoGallerySection.style.display = 'none';
		}
	}

	function attachPhotoEventListeners() {
		document.querySelectorAll('.delete-photo-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const photoId = btn.dataset.photoId;
				if (confirm('Delete this photo?')) {
					deletePhoto(photoId);
				}
			});
		});

	}

	// Delete Photo
	async function deletePhoto(photoId) {
		try {
			const response = await fetch(config.endpoints.deletePhoto, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/x-www-form-urlencoded'
				},
				body: `photoId=${photoId}&__RequestVerificationToken=${encodeURIComponent(getCSRFToken())}`
			});

			if (!response.ok) {
				throw new Error('Failed to delete photo');
			}

			loadFeed();
			showNotification('Photo deleted.', 'success');
		} catch (error) {
			showNotification('Could not delete the photo. Please try again.', 'error');
		}
	}

	// Utilities
	function escapeHtml(text) {
		text = text == null ? '' : String(text);
		const map = {
			'&': '&amp;',
			'<': '&lt;',
			'>': '&gt;',
			'"': '&quot;',
			"'": '&#039;'
		};
		return text.replace(/[&<>"']/g, m => map[m]);
	}

	function getCSRFToken() {
		const token = document.querySelector('input[name="__RequestVerificationToken"]');
		return token ? token.value : '';
	}

	function getTimeAgo(date) {
		const now = new Date();
		const seconds = Math.floor((now - date) / 1000);

		if (seconds < 60) return 'just now';
		if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
		if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
		if (seconds < 604800) return `${Math.floor(seconds / 86400)}d ago`;

		return date.toLocaleDateString();
	}

	function showNotification(message, type = 'info') {
		const notification = document.createElement('div');
		notification.className = `alert ${type === 'success' ? 'alert-success' : type === 'error' ? 'alert-danger' : 'alert-info'} app-toast`;
		notification.setAttribute('role', type === 'error' ? 'alert' : 'status');
		notification.textContent = message;
		document.body.appendChild(notification);

		setTimeout(() => notification.classList.add('app-toast--hide'), 2600);
		setTimeout(() => notification.remove(), 3000);
	}

	// Start the app
	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', init);
	} else {
		init();
	}
})();
