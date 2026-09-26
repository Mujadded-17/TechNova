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
		pendingMedia: [],   // staged composer media: { type, file, previewUrl, name, size }
		isSubmitting: false,
		activeMediaTab: 'photos',
		// Lightbox state
		lightboxImages: [],
		currentLightboxIndex: 0
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
		mediaPreview: document.getElementById('postMediaPreview'),
		modalAddPhotoBtn: document.getElementById('modalAddPhotoBtn'),
		modalAddVideoBtn: document.getElementById('modalAddVideoBtn'),
		photoGallery: document.getElementById('photoGallery'),
		videoGallery: document.getElementById('videoGallery'),
		mediaGallerySection: document.getElementById('mediaGallerySection'),
		mediaTabPhotos: document.getElementById('mediaTabPhotos'),
		mediaTabVideos: document.getElementById('mediaTabVideos'),
		photoTabCount: document.getElementById('photoTabCount'),
		videoTabCount: document.getElementById('videoTabCount'),
		uploadMediaBtn: document.getElementById('uploadMediaBtn'),
		// Lightbox elements
		photoLightbox: document.getElementById('photoLightbox'),
		lightboxImage: document.getElementById('lightboxImage'),
		lightboxCaption: document.getElementById('lightboxCaption'),
		lightboxCounter: document.getElementById('lightboxCounter'),
		lightboxClose: document.getElementById('lightboxClose'),
		lightboxPrev: document.getElementById('lightboxPrev'),
		lightboxNext: document.getElementById('lightboxNext'),
		lightboxBackdrop: document.querySelector('.lightbox-backdrop')
	};

	// Initialize
	function init() {
		attachEventListeners();
		loadFeed();
	}

	// Event Listeners
	function attachEventListeners() {
		// Dashboard card: Facebook opens the composer and arms the file
		// picker in a single tap.
		elements.postBtn.addEventListener('click', () => openPostModal());
		elements.openCreatePost.addEventListener('click', () => openPostModal());
		elements.postPhotoBtn.addEventListener('click', () => openPostModal('photo'));
		elements.postVideoBtn.addEventListener('click', () => openPostModal('video'));
		elements.modalAddPhotoBtn.addEventListener('click', () => elements.photoFileInput.click());
		elements.modalAddVideoBtn.addEventListener('click', () => elements.videoFileInput.click());
		elements.photoFileInput.addEventListener('change', handlePhotoSelection);
		elements.videoFileInput.addEventListener('change', handleVideoSelection);
		elements.closePostModal.addEventListener('click', closePostModal);
		elements.cancelPostModal.addEventListener('click', closePostModal);
		elements.submitPost.addEventListener('click', submitPost);
		elements.postModalContent.addEventListener('input', updatePostButtonState);
		// The media library Upload button opens the composer with the picker
		// armed for whichever tab is active (photo or video).
		elements.uploadMediaBtn.addEventListener('click', () => openPostModal(state.activeMediaTab === 'videos' ? 'video' : 'photo'));
		elements.mediaTabPhotos.addEventListener('click', () => switchMediaTab('photos'));
		elements.mediaTabVideos.addEventListener('click', () => switchMediaTab('videos'));

		// Lightbox event listeners
		elements.lightboxClose.addEventListener('click', closeLightbox);
		elements.lightboxBackdrop.addEventListener('click', closeLightbox);
		elements.lightboxPrev.addEventListener('click', previousLightboxImage);
		elements.lightboxNext.addEventListener('click', nextLightboxImage);

		// Keyboard shortcuts
		document.addEventListener('keydown', (e) => {
			if (e.key === 'Escape') {
				if (elements.photoLightbox.style.display !== 'none') {
					closeLightbox();
				} else {
					closePostModal();
				}
			} else if (e.key === 'ArrowLeft' && elements.photoLightbox.style.display !== 'none') {
				previousLightboxImage();
			} else if (e.key === 'ArrowRight' && elements.photoLightbox.style.display !== 'none') {
				nextLightboxImage();
			}
		});
	}

	// Post Modal Functions
	function openPostModal(pick) {
		// Carry over anything typed in the dashboard card input.
		if (elements.postContentInput.value.trim() && !elements.postModalContent.value.trim()) {
			elements.postModalContent.value = elements.postContentInput.value.trim();
			elements.postContentInput.value = '';
		}

		elements.postModal.classList.add('active');
		updatePostButtonState();
		elements.postModalContent.focus();

		if (pick === 'photo') {
			elements.photoFileInput.click();
		} else if (pick === 'video') {
			elements.videoFileInput.click();
		}
	}

	function closePostModal() {
		elements.postModal.classList.remove('active');
		resetComposer();
	}

	function resetComposer() {
		elements.postModalContent.value = '';
		elements.submitPost.textContent = 'Post';
		delete elements.submitPost.dataset.postId;
		clearPendingMedia();
		updatePostButtonState();
	}

	// Photo/Video Selection — Facebook-style: validate, preview, and stage
	// everything in the composer before anything is uploaded.
	const MEDIA_LIMITS = {
		photo: { maxSizeBytes: 10 * 1024 * 1024, extensions: ['jpg', 'jpeg', 'png', 'gif', 'webp'], maxCount: 10 },
		video: { maxSizeBytes: 100 * 1024 * 1024, extensions: ['mp4', 'webm', 'avi', 'mov', 'mkv'], maxCount: 1 }
	};

	function getFileExtension(fileName) {
		const index = fileName.lastIndexOf('.');
		return index === -1 ? '' : fileName.slice(index + 1).toLowerCase();
	}

	function formatFileSize(bytes) {
		if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)}MB`;
		if (bytes >= 1024) return `${Math.round(bytes / 1024)}KB`;
		return `${bytes}B`;
	}

	function handlePhotoSelection(e) {
		addPendingMedia(e.target.files, 'photo');
		e.target.value = '';
	}

	function handleVideoSelection(e) {
		addPendingMedia(e.target.files, 'video');
		e.target.value = '';
	}

	function addPendingMedia(fileList, kind) {
		const rule = MEDIA_LIMITS[kind];
		const kindLabel = kind === 'photo' ? 'photo' : 'video';

		Array.from(fileList).forEach(file => {
			const ext = getFileExtension(file.name);
			if (!rule.extensions.includes(ext)) {
				showNotification(`"${file.name}" is not a supported ${kindLabel} type.`, 'error');
				return;
			}
			if (file.size > rule.maxSizeBytes) {
				showNotification(`"${file.name}" is too large. Max ${kindLabel} size is ${formatFileSize(rule.maxSizeBytes)}.`, 'error');
				return;
			}
			const count = state.pendingMedia.filter(m => m.type === kind).length;
			if (count >= rule.maxCount) {
				showNotification(kind === 'photo'
					? `You can attach up to ${rule.maxCount} photos per post.`
					: 'Only one video can be attached per post.', 'error');
				return;
			}
			state.pendingMedia.push({
				type: kind,
				file: file,
				previewUrl: URL.createObjectURL(file),
				name: file.name,
				size: file.size
			});
		});

		renderMediaPreview();
		updatePostButtonState();
	}

	function renderMediaPreview() {
		const media = state.pendingMedia;

		if (media.length === 0) {
			elements.mediaPreview.style.display = 'none';
			elements.mediaPreview.innerHTML = '';
			return;
		}

		elements.mediaPreview.style.display = 'grid';
		elements.mediaPreview.innerHTML = media.map((item, index) => `
			<div class="composer-media-item">
				${item.type === 'photo'
					? `<img src="${item.previewUrl}" alt="${escapeHtml(item.name)}" />`
					: `<video src="${item.previewUrl}" muted autoplay loop playsinline></video>`}
				<button type="button" class="composer-media-remove" data-index="${index}" title="Remove">×</button>
			</div>
		`).join('');

		elements.mediaPreview.querySelectorAll('.composer-media-remove').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const removed = state.pendingMedia.splice(parseInt(btn.dataset.index, 10), 1);
				removed.forEach(item => URL.revokeObjectURL(item.previewUrl));
				renderMediaPreview();
				updatePostButtonState();
			});
		});
	}

	function clearPendingMedia() {
		state.pendingMedia.forEach(item => URL.revokeObjectURL(item.previewUrl));
		state.pendingMedia = [];
		renderMediaPreview();
	}

	function updatePostButtonState() {
		if (state.isSubmitting) return;
		const hasContent = elements.postModalContent.value.trim().length > 0;
		const hasMedia = state.pendingMedia.length > 0;
		const editing = Boolean(elements.submitPost.dataset.postId);
		elements.submitPost.disabled = !editing && !(hasContent || hasMedia);
	}

	// Submit Post — creates the post first so staged media can attach to its
	// id, then uploads each file with visible progress and failure feedback.
	async function submitPost() {
		const content = elements.postModalContent.value.trim();
		const editId = elements.submitPost.dataset.postId || null;

		if (!content && state.pendingMedia.length === 0 && !editId) {
			return;
		}

		state.isSubmitting = true;
		elements.submitPost.disabled = true;

		try {
			if (editId) {
				// Editing keeps the composer content-only; media is untouched.
				const response = await fetch(config.endpoints.editPost, {
					method: 'POST',
					headers: {
						'Content-Type': 'application/x-www-form-urlencoded'
					},
					body: `postId=${encodeURIComponent(editId)}&content=${encodeURIComponent(content)}&__RequestVerificationToken=${encodeURIComponent(getCSRFToken())}`
				});

				if (!response.ok) {
					throw new Error('Failed to update post');
				}

				showNotification('Post updated successfully!', 'success');
			} else {
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

				const uploads = state.pendingMedia.map(item => ({
					item: item,
					endpoint: item.type === 'photo' ? config.endpoints.uploadPhoto : config.endpoints.uploadVideo,
					fields: item.type === 'photo'
						? [['caption', ''], ['postId', String(postId)]]
						: [['title', ''], ['description', ''], ['postId', String(postId)]]
				}));

				let uploaded = 0;
				const failures = [];

				for (const upload of uploads) {
					elements.submitPost.textContent = `Uploading ${uploaded + 1}/${uploads.length}...`;

					const formData = new FormData();
					formData.append('file', upload.item.file);
					upload.fields.forEach(([key, value]) => formData.append(key, value));
					formData.append('__RequestVerificationToken', getCSRFToken());

					try {
						const response = await fetch(upload.endpoint, {
							method: 'POST',
							body: formData
						});

						if (!response.ok) {
							const message = await response.text().catch(() => '');
							throw new Error(message || `HTTP ${response.status}`);
						}
					} catch (error) {
						console.error(`Failed to upload ${upload.item.type}:`, error);
						failures.push(`${upload.item.type} "${upload.item.name}" (${error.message})`);
					}

					uploaded++;
				}

				if (failures.length > 0) {
					showNotification(`Post created, but ${failures.length} file(s) failed to upload. First failure: ${failures[0]}`, 'error');
				} else {
					showNotification('Post created successfully!', 'success');
				}
			}

			closePostModal();
			loadFeed();
		} catch (error) {
			console.error('Error submitting post:', error);
			showNotification(`Failed to submit post: ${error.message}`, 'error');
		} finally {
			state.isSubmitting = false;
			elements.submitPost.disabled = false;
			elements.submitPost.textContent = 'Post';
			delete elements.submitPost.dataset.postId;
			updatePostButtonState();
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
			loadMedia();
			// Attach photo click handlers for lightbox
			setTimeout(attachPhotoClickHandlers, 0);
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

	// The dashboard feed only ever shows the signed-in startup's own posts,
	// so the author name/logo come from the #postsFeed data attributes that
	// Dashboard.cshtml renders. Razor does not run inside this static file —
	// a literal "@Model.CompanyName" here is displayed as plain text.
	function getProfile() {
		if (!getProfile.cache) {
			const feed = document.getElementById('postsFeed');
			getProfile.cache = {
				name: (feed && feed.dataset.companyName) || 'Your Startup',
				logo: (feed && feed.dataset.companyLogo) || ''
			};
		}
		return getProfile.cache;
	}

	// SQL Server drops the UTC kind on datetimes, so JSON timestamps reach
	// the browser without a zone suffix and get misread as local time —
	// which made brand-new posts look hours old. Treat zone-less values
	// as UTC.
	function parseFeedDate(value) {
		if (!value) return new Date();
		if (typeof value === 'string' && !/(Z|[+-]\d{2}:?\d{2})$/.test(value)) {
			return new Date(value + 'Z');
		}
		return new Date(value);
	}

	function renderPostCard(post) {
		const profile = getProfile();
		const date = parseFeedDate(post.createdAt);
		const timeAgo = getTimeAgo(date);
		const fullTime = date.toLocaleString(undefined, {
			day: 'numeric',
			month: 'short',
			year: 'numeric',
			hour: 'numeric',
			minute: '2-digit'
		});
		const photos = post.photos || [];
		const videos = post.videos || [];
		const avatar = profile.logo
			? `<img src="${escapeHtml(profile.logo)}" alt="${escapeHtml(profile.name)} logo" />`
			: '';

		return `
			<div class="feed-post fade-in" data-post-id="${post.postID}">
				<div class="post-header">
					<div class="post-author-info">
						<div class="post-avatar">${avatar}</div>
						<div>
							<h4>${escapeHtml(profile.name)}</h4>
							<p class="post-time" title="${fullTime}">${timeAgo}</p>
						</div>
					</div>
					<button type="button" class="post-menu-btn" data-post-id="${post.postID}">⋮</button>
				</div>
				${post.content ? `<div class="post-content">${escapeHtml(post.content)}</div>` : ''}
				${(photos.length > 0 || videos.length > 0) ? `
					<div class="post-media">
						${photos.map(photo => `
							<img src="${escapeHtml(photo.filePath)}" alt="Post photo" />
						`).join('')}
						${videos.map(video => `
							<video controls preload="metadata">
								<source src="${escapeHtml(video.filePath)}" type="${escapeHtml(video.mimeType || 'video/mp4')}">
								Your browser does not support the video tag.
							</video>
						`).join('')}
					</div>
				` : ''}
				<div class="post-interactions">
					<div class="post-stats">
						<span class="like-count" data-post-id="${post.postID}">0 likes</span>
						<span class="comment-count" data-post-id="${post.postID}">0 comments</span>
					</div>
					<div class="post-actions">
						<button type="button" class="post-action-btn like-btn" data-post-id="${post.postID}" title="Like this post">
							<span class="like-icon">👍</span> Like
						</button>
						<button type="button" class="post-action-btn comment-btn" data-post-id="${post.postID}" title="Comment on this post">
							<span class="comment-icon">💬</span> Comment
						</button>
					</div>
				</div>
				<div class="post-comments-section" data-post-id="${post.postID}" style="display: none;">
					<div class="comments-list"></div>
					<div class="comment-input-area">
						<input type="text" class="comment-input" placeholder="Write a comment..." />
						<button type="button" class="submit-comment-btn">Post</button>
					</div>
				</div>
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

		// Like button handlers
		document.querySelectorAll('.like-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const postId = btn.dataset.postId;
				likePost(postId, btn);
			});
		});

		// Comment button handlers
		document.querySelectorAll('.comment-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const postId = btn.dataset.postId;
				const commentsSection = document.querySelector(`.post-comments-section[data-post-id="${postId}"]`);
				if (commentsSection) {
					const isShowing = commentsSection.style.display !== 'none';
					commentsSection.style.display = isShowing ? 'none' : 'block';
					if (!isShowing) {
						loadComments(postId);
					}
				}
			});
		});

		// Comment submit handlers
		document.querySelectorAll('.submit-comment-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const commentsSection = btn.closest('.post-comments-section');
				const postId = commentsSection.dataset.postId;
				const input = commentsSection.querySelector('.comment-input');
				const content = input.value.trim();
				if (content) {
					submitComment(postId, content, input, commentsSection);
				}
			});
		});

		// Load like counts and comment counts on render
		document.querySelectorAll('.feed-post').forEach(postCard => {
			const postId = postCard.dataset.postId;
			updatePostStats(postId);
		});
	}

	// Edit Post
	function editPost(post) {
		clearPendingMedia();
		elements.postModalContent.value = post.content || '';
		elements.submitPost.dataset.postId = post.postID;
		elements.submitPost.textContent = 'Update Post';
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
			showNotification('Post deleted successfully', 'success');
		} catch (error) {
			console.error('Error deleting post:', error);
			showNotification('Failed to delete post', 'error');
		}
	}

	// Load and Render the Media Library (Photos / Videos tabs)
	async function loadMedia() {
		try {
			const response = await fetch(config.endpoints.getFeed);
			if (!response.ok) {
				throw new Error('Failed to load media');
			}

			const posts = await response.json();
			let allPhotos = [];
			let allVideos = [];

			// Collect all media attached to posts
			(posts || []).forEach(post => {
				if (post.photos && post.photos.length > 0) {
					allPhotos = allPhotos.concat(post.photos);
				}
				if (post.videos && post.videos.length > 0) {
					allVideos = allVideos.concat(post.videos);
				}
			});

			if (allPhotos.length === 0 && allVideos.length === 0) {
				elements.mediaGallerySection.style.display = 'none';
				return;
			}

			elements.mediaGallerySection.style.display = 'block';
			elements.photoTabCount.textContent = allPhotos.length;
			elements.videoTabCount.textContent = allVideos.length;

			elements.photoGallery.innerHTML = allPhotos.length > 0
				? allPhotos.map(photo => `
					<div class="gallery-item" data-photo-id="${photo.photoID}">
						<img src="${escapeHtml(photo.filePath)}" alt="${escapeHtml(photo.caption || 'Gallery photo')}" />
						<div class="gallery-item-overlay">
							<button type="button" class="edit-photo-btn" data-photo-id="${photo.photoID}" title="Edit">✏️</button>
							<button type="button" class="delete-photo-btn" data-photo-id="${photo.photoID}" title="Delete">🗑️</button>
						</div>
					</div>
				`).join('')
				: '<p class="media-empty-note">No photos yet — use the composer above to post some.</p>';

			elements.videoGallery.innerHTML = allVideos.length > 0
				? allVideos.map(video => `
					<div class="gallery-item" data-video-id="${video.videoID}">
						<video src="${escapeHtml(video.filePath)}" preload="metadata" muted></video>
						<div class="gallery-item-overlay">
							<button type="button" class="delete-video-btn" data-video-id="${video.videoID}" title="Delete">🗑️</button>
						</div>
					</div>
				`).join('')
				: '<p class="media-empty-note">No videos yet — use the composer above to post one.</p>';

			// Keep the visitor on their current tab unless it is now empty.
			if (state.activeMediaTab === 'videos' && allVideos.length === 0 && allPhotos.length > 0) {
				state.activeMediaTab = 'photos';
			} else if (state.activeMediaTab === 'photos' && allPhotos.length === 0) {
				state.activeMediaTab = 'videos';
			}
			switchMediaTab(state.activeMediaTab);

			attachMediaEventListeners();
		} catch (error) {
			console.error('Error loading media:', error);
		}
	}

	function switchMediaTab(tab) {
		state.activeMediaTab = tab;
		const isPhotos = tab === 'photos';
		elements.mediaTabPhotos.classList.toggle('active', isPhotos);
		elements.mediaTabVideos.classList.toggle('active', !isPhotos);
		elements.photoGallery.style.display = isPhotos ? 'grid' : 'none';
		elements.videoGallery.style.display = isPhotos ? 'none' : 'grid';
	}

	function attachMediaEventListeners() {
		document.querySelectorAll('.delete-photo-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const photoId = btn.dataset.photoId;
				if (confirm('Delete this photo?')) {
					deletePhoto(photoId);
				}
			});
		});

		document.querySelectorAll('.delete-video-btn').forEach(btn => {
			btn.addEventListener('click', (e) => {
				e.preventDefault();
				const videoId = btn.dataset.videoId;
				if (confirm('Delete this video?')) {
					deleteVideo(videoId);
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

			loadMedia();
			showNotification('Photo deleted successfully', 'success');
		} catch (error) {
			console.error('Error deleting photo:', error);
			showNotification('Failed to delete photo', 'error');
		}
	}

	// Delete Video
	async function deleteVideo(videoId) {
		try {
			const response = await fetch(config.endpoints.deleteVideo, {
				method: 'POST',
				headers: {
					'Content-Type': 'application/x-www-form-urlencoded'
				},
				body: `videoId=${videoId}&__RequestVerificationToken=${encodeURIComponent(getCSRFToken())}`
			});

			if (!response.ok) {
				throw new Error('Failed to delete video');
			}

			loadMedia();
			showNotification('Video deleted successfully', 'success');
		} catch (error) {
			console.error('Error deleting video:', error);
			showNotification('Failed to delete video', 'error');
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
		return String(text ?? '').replace(/[&<>"']/g, m => map[m]);
	}

	function getCSRFToken() {
		const token = document.querySelector('input[name="__RequestVerificationToken"]');
		return token ? token.value : '';
	}

	function getTimeAgo(date) {
		const now = new Date();
		const seconds = Math.floor((now - date) / 1000);

		if (isNaN(seconds)) return '';
		if (seconds < 60) return 'just now';
		if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`;
		if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`;
		if (seconds < 604800) return `${Math.floor(seconds / 86400)}d ago`;

		// Older posts: show the real date and time, not just a bare date.
		return date.toLocaleString(undefined, {
			day: 'numeric',
			month: 'short',
			year: 'numeric',
			hour: 'numeric',
			minute: '2-digit'
		});
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

	// ===== LIGHTBOX FUNCTIONS =====

	// Open lightbox with image collection
	function openLightbox(images, startIndex = 0) {
		state.lightboxImages = images.map(img => ({
			src: typeof img === 'string' ? img : img.src,
			caption: typeof img === 'string' ? '' : (img.caption || '')
		}));
		state.currentLightboxIndex = Math.min(startIndex, state.lightboxImages.length - 1);

		if (state.lightboxImages.length === 0) return;

		displayLightboxImage();
		elements.photoLightbox.style.display = 'flex';
		document.body.style.overflow = 'hidden';

		// Hide/show navigation buttons based on image count
		if (state.lightboxImages.length === 1) {
			elements.lightboxPrev.classList.add('hidden');
			elements.lightboxNext.classList.add('hidden');
		} else {
			elements.lightboxPrev.classList.remove('hidden');
			elements.lightboxNext.classList.remove('hidden');
		}
	}

	// Close the lightbox
	function closeLightbox() {
		elements.photoLightbox.style.display = 'none';
		document.body.style.overflow = '';
		state.lightboxImages = [];
		state.currentLightboxIndex = 0;
	}

	// Display current lightbox image
	function displayLightboxImage() {
		const image = state.lightboxImages[state.currentLightboxIndex];
		if (!image) return;

		elements.lightboxImage.src = image.src;
		elements.lightboxCaption.textContent = image.caption || '';
		elements.lightboxCounter.textContent = `${state.currentLightboxIndex + 1} / ${state.lightboxImages.length}`;

		// Update button states
		elements.lightboxPrev.disabled = state.currentLightboxIndex === 0;
		elements.lightboxNext.disabled = state.currentLightboxIndex === state.lightboxImages.length - 1;
	}

	// Navigate to previous image
	function previousLightboxImage() {
		if (state.currentLightboxIndex > 0) {
			state.currentLightboxIndex--;
			displayLightboxImage();
		}
	}

	// Navigate to next image
	function nextLightboxImage() {
		if (state.currentLightboxIndex < state.lightboxImages.length - 1) {
			state.currentLightboxIndex++;
			displayLightboxImage();
		}
	}

	// Attach photo click handlers after loading feed
	function attachPhotoClickHandlers() {
		// Cover photo
		const coverPhoto = document.getElementById('coverPhotoClickable');
		if (coverPhoto && coverPhoto.src) {
			coverPhoto.addEventListener('click', () => {
				openLightbox([{ src: coverPhoto.src, caption: 'Cover photo' }]);
			});
		}

		// Logo photo
		const logoPhoto = document.getElementById('logoPhotoClickable');
		if (logoPhoto && logoPhoto.src) {
			logoPhoto.addEventListener('click', () => {
				openLightbox([{ src: logoPhoto.src, caption: 'Profile photo' }]);
			});
		}

		// Gallery items
		document.querySelectorAll('.gallery-item').forEach(item => {
			const img = item.querySelector('img');
			if (img && img.src) {
				item.addEventListener('click', (e) => {
					if (e.target.closest('button')) return; // Don't open lightbox if clicking edit/delete
					const allPhotos = Array.from(document.querySelectorAll('.gallery-item img'))
						.filter(i => i.src)
						.map(i => ({ src: i.src, caption: i.alt || '' }));
					const index = Array.from(document.querySelectorAll('.gallery-item img')).indexOf(img);
					openLightbox(allPhotos, index);
				});
			}
		});

		// Post images
		document.querySelectorAll('.post-media img').forEach(img => {
			if (img.src) {
				img.style.cursor = 'pointer';
				img.addEventListener('click', () => {
					const postCard = img.closest('.feed-post');
					if (postCard) {
						const allImages = Array.from(postCard.querySelectorAll('.post-media img'))
							.filter(i => i.src)
							.map(i => ({ src: i.src, caption: '' }));
						const index = Array.from(postCard.querySelectorAll('.post-media img')).indexOf(img);
						openLightbox(allImages, index);
					}
				});
			}
		});
	}

	// Override loadFeed to include photo handlers
	const originalLoadFeed = window.loadFeed || loadFeed;
	async function loadFeedWithPhotoHandlers() {
		await originalLoadFeed();
		// Wait a tick for DOM to update, then attach handlers
		setTimeout(attachPhotoClickHandlers, 0);
	}

	// ===== LIKE/COMMENT FUNCTIONS =====

	async function updatePostStats(postId) {
		try {
			// Get like count
			const likeResponse = await fetch(`/Startup/GetPostLikes?postId=${postId}`);
			const likeData = await likeResponse.json();

			// Get comment count
			const commentResponse = await fetch(`/Startup/GetPostComments?postId=${postId}`);
			const comments = await commentResponse.json();

			// Update UI
			const postCard = document.querySelector(`.feed-post[data-post-id="${postId}"]`);
			if (postCard) {
				const likeCountEl = postCard.querySelector('.like-count');
				const commentCountEl = postCard.querySelector('.comment-count');
				const likeBtn = postCard.querySelector('.like-btn');

				if (likeCountEl) {
					likeCountEl.textContent = `${likeData.likeCount} ${likeData.likeCount === 1 ? 'like' : 'likes'}`;
				}
				if (commentCountEl) {
					commentCountEl.textContent = `${comments.length} ${comments.length === 1 ? 'comment' : 'comments'}`;
				}

				// Update like button state
				if (likeBtn) {
					if (likeData.hasLiked) {
						likeBtn.classList.add('liked');
						likeBtn.style.color = '#0a66c2';
					} else {
						likeBtn.classList.remove('liked');
						likeBtn.style.color = '';
					}
				}
			}
		} catch (error) {
			console.error('Error updating post stats:', error);
		}
	}

	async function likePost(postId, btnElement) {
		try {
			const response = await fetch('/Startup/LikePost', {
				method: 'POST',
				headers: {
					'Content-Type': 'application/x-www-form-urlencoded',
					'RequestVerificationToken': getCSRFToken()
				},
				body: `postId=${postId}`
			});

			if (response.ok) {
				const data = await response.json();
				updatePostStats(postId);
			} else {
				if (response.status === 401) {
					showNotification('Please login as an investor to like posts', 'error');
				} else {
					showNotification('Failed to like post', 'error');
				}
			}
		} catch (error) {
			console.error('Error liking post:', error);
			showNotification('Error liking post', 'error');
		}
	}

	async function loadComments(postId) {
		try {
			const response = await fetch(`/Startup/GetPostComments?postId=${postId}`);
			const comments = await response.json();

			const postCard = document.querySelector(`.feed-post[data-post-id="${postId}"]`);
			if (postCard) {
				const commentsList = postCard.querySelector('.comments-list');
				if (commentsList) {
					commentsList.innerHTML = comments.map(comment => `
						<div class="comment-item">
							<strong>${escapeHtml(comment.investorName)}</strong>
							<p>${escapeHtml(comment.content)}</p>
							<span class="comment-time">${getTimeAgo(new Date(comment.createdAt))}</span>
						</div>
					`).join('');
				}
			}
		} catch (error) {
			console.error('Error loading comments:', error);
		}
	}

	async function submitComment(postId, content, inputElement, commentsSectionElement) {
		try {
			const formData = new FormData();
			formData.append('postId', postId);
			formData.append('content', content);

			const response = await fetch('/Startup/AddComment', {
				method: 'POST',
				headers: {
					'RequestVerificationToken': getCSRFToken()
				},
				body: formData
			});

			if (response.ok) {
				const data = await response.json();
				inputElement.value = '';
				loadComments(postId);
				updatePostStats(postId);
				showNotification('Comment posted!', 'success');
			} else {
				if (response.status === 401) {
					showNotification('Please login as an investor to comment', 'error');
				} else {
					showNotification('Failed to post comment', 'error');
				}
			}
		} catch (error) {
			console.error('Error submitting comment:', error);
			showNotification('Error posting comment', 'error');
		}
	}

})();
