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
		pendingPhotos: []
	};

	// DOM Elements
	const elements = {
		postBtn: document.getElementById('postBtn'),
		postContentInput: document.getElementById('postContentInput'),
		postPhotoBtn: document.getElementById('postPhotoBtn'),
		postVideoBtn: document.getElementById('postVideoBtn'),
		photoFileInput: document.getElementById('photoFileInput'),
		videoFileInput: document.getElementById('videoFileInput'),
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
		photoGallerySection: document.getElementById('photoGallerySection'),
		uploadPhotoBtn: document.getElementById('uploadPhotoBtn')
	};

	// Initialize
	function init() {
		attachEventListeners();
		loadFeed();
	}

	// Event Listeners
	function attachEventListeners() {
		elements.postBtn.addEventListener('click', () => openPostModal());
		elements.openCreatePost.addEventListener('click', () => openPostModal());
		elements.postPhotoBtn.addEventListener('click', () => elements.photoFileInput.click());
		elements.postVideoBtn.addEventListener('click', () => elements.videoFileInput.click());
		elements.photoFileInput.addEventListener('change', handlePhotoSelection);
		elements.videoFileInput.addEventListener('change', handleVideoSelection);
		elements.closePostModal.addEventListener('click', closePostModal);
		elements.cancelPostModal.addEventListener('click', closePostModal);
		elements.submitPost.addEventListener('click', submitPost);
		elements.uploadPhotoBtn.addEventListener('click', () => elements.photoFileInput.click());

		// Keyboard shortcuts
		document.addEventListener('keydown', (e) => {
			if (e.key === 'Escape') {
				closePostModal();
			}
		});
	}

	// Post Modal Functions
	function openPostModal() {
		elements.postModal.classList.add('active');
		elements.postModalContent.focus();
	}

	function closePostModal() {
		elements.postModal.classList.remove('active');
		elements.postModalContent.value = '';
		clearPendingPhotos();
	}

	// Photo/Video Selection
	function handlePhotoSelection(e) {
		const files = Array.from(e.target.files);
		files.forEach(file => {
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

	function handleVideoSelection(e) {
		const file = e.target.files[0];
		if (file) {
			console.log('Video selected:', file.name);
			// Video upload will be handled separately after post creation
		}
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
			alert('Please enter some content for your post');
			return;
		}

		try {
			elements.submitPost.disabled = true;
			elements.submitPost.textContent = 'Posting...';

			// Create the post
			const createResponse = await fetch(config.endpoints.createPost, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/x-www-form-urlencoded'
				},
				body: `content=${encodeURIComponent(content)}&__RequestVerificationToken=${encodeURIComponent(getCSRFToken())}`
			});

			if (!createResponse.ok) {
				const errorText = await createResponse.text();
				throw new Error(`Failed to create post: ${createResponse.status} - ${errorText}`);
			}

			const postData = await createResponse.json();
			const postId = postData.postId;

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
						console.error(`Failed to upload photo: ${uploadResponse.status}`);
					}
				}
			}

			closePostModal();
			loadFeed();
			showNotification('Post created successfully!', 'success');
		} catch (error) {
			console.error('Error creating post:', error);
			showNotification(`Failed to create post: ${error.message}`, 'error');
		} finally {
			elements.submitPost.disabled = false;
			elements.submitPost.textContent = 'Post';
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
			loadPhotos();
		} catch (error) {
			console.error('Error loading feed:', error);
		}
	}

	// Render Feed
	function renderFeed(posts) {
		if (!posts || posts.length === 0) {
			elements.postsFeed.innerHTML = `
				<div class="empty-state">
					<div class="empty-state-icon">📝</div>
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

		return `
			<div class="feed-post fade-in" data-post-id="${post.postID}">
				<div class="post-header">
					<div class="post-author-info">
						<div class="post-avatar"></div>
						<div>
							<h4>@Model.CompanyName</h4>
							<p class="post-time">${timeAgo}</p>
						</div>
					</div>
					<button type="button" class="post-menu-btn" data-post-id="${post.postID}">⋮</button>
				</div>
				<div class="post-content">${escapeHtml(post.content)}</div>
				${photos.length > 0 ? `
					<div class="post-media">
						${photos.map(photo => `
							<img src="${photo.filePath}" alt="Photo" />
						`).join('')}
					</div>
				` : ''}
				<div class="post-footer">
					<button type="button" class="edit-post-btn" data-post-id="${post.postID}">✏️ Edit</button>
					<button type="button" class="delete-post-btn" data-post-id="${post.postID}">🗑️ Delete</button>
				</div>
			</div>
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
		elements.postModalContent.value = post.content;
		openPostModal();
		elements.submitPost.dataset.postId = post.postID;
		elements.submitPost.textContent = 'Update Post';
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
			showNotification('Post deleted successfully', 'success');
		} catch (error) {
			console.error('Error deleting post:', error);
			showNotification('Failed to delete post', 'error');
		}
	}

	// Load and Render Photos
	async function loadPhotos() {
		try {
			const response = await fetch(config.endpoints.getFeed);
			if (!response.ok) {
				throw new Error('Failed to load photos');
			}

			const posts = await response.json();
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
					<img src="${photo.filePath}" alt="${photo.caption || 'Gallery photo'}" />
					<div class="gallery-item-overlay">
						<button type="button" class="edit-photo-btn" data-photo-id="${photo.photoID}" title="Edit">✏️</button>
						<button type="button" class="delete-photo-btn" data-photo-id="${photo.photoID}" title="Delete">🗑️</button>
					</div>
				</div>
			`).join('');

			attachPhotoEventListeners();
		} catch (error) {
			console.error('Error loading photos:', error);
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

		document.querySelectorAll('.edit-photo-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const photoId = btn.dataset.photoId;
				prompt('Enter new caption for this photo:', '');
				// Implement caption editing if needed
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

			loadPhotos();
			showNotification('Photo deleted successfully', 'success');
		} catch (error) {
			console.error('Error deleting photo:', error);
			showNotification('Failed to delete photo', 'error');
		}
	}

	// Utilities
	function escapeHtml(text) {
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
		notification.style.cssText = `
			position: fixed;
			top: 20px;
			right: 20px;
			background: ${type === 'success' ? '#0f7a5c' : '#c0392b'};
			color: white;
			padding: 1rem 1.5rem;
			border-radius: 8px;
			box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15);
			z-index: 10000;
			animation: slideIn 0.3s ease-out;
			font-weight: 500;
		`;
		notification.textContent = message;
		document.body.appendChild(notification);

		setTimeout(() => {
			notification.style.animation = 'fadeOut 0.3s ease-out';
			setTimeout(() => notification.remove(), 300);
		}, 3000);
	}

	// Start the app
	if (document.readyState === 'loading') {
		document.addEventListener('DOMContentLoaded', init);
	} else {
		init();
	}
})();
