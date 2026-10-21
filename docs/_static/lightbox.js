/* Site-wide click-to-zoom lightbox for content images (screenshots/gifs).
   Click any image in the main content area for a near-full-size view;
   dismiss with another click, the close button, or Escape. No dependency
   on a Sphinx extension — just this file plus lightbox.css, registered via
   html_css_files/html_js_files in conf.py, so nothing new needs to sync
   into reqnroll/Reqnroll's own build. */

document.addEventListener('DOMContentLoaded', function () {
  var overlay = document.createElement('div');
  overlay.className = 'reqnroll-lightbox-overlay';
  overlay.setAttribute('role', 'dialog');
  overlay.setAttribute('aria-modal', 'true');
  overlay.setAttribute('aria-label', 'Image preview');

  var closeBtn = document.createElement('button');
  closeBtn.type = 'button';
  closeBtn.className = 'reqnroll-lightbox-close';
  closeBtn.setAttribute('aria-label', 'Close');
  closeBtn.innerHTML = '&times;';

  var img = document.createElement('img');
  img.alt = '';

  overlay.appendChild(closeBtn);
  overlay.appendChild(img);
  document.body.appendChild(overlay);

  var lastFocused = null;

  function open(src, alt) {
    lastFocused = document.activeElement;
    img.src = src;
    img.alt = alt || '';
    overlay.classList.add('is-open');
    document.body.style.overflow = 'hidden';
    closeBtn.focus();
  }

  function close() {
    overlay.classList.remove('is-open');
    img.src = '';
    document.body.style.overflow = '';
    if (lastFocused && typeof lastFocused.focus === 'function') {
      lastFocused.focus();
    }
  }

  overlay.addEventListener('click', function (event) {
    // Only the backdrop or the close button dismiss it -- a click on the
    // enlarged image itself (or a future caption/toolbar) should not.
    if (event.target === overlay || event.target === closeBtn) {
      close();
    }
  });

  document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape' && overlay.classList.contains('is-open')) {
      close();
    }
  });

  var contentImages = document.querySelectorAll('[role="main"] img');
  contentImages.forEach(function (image) {
    image.addEventListener('click', function () {
      open(image.currentSrc || image.src, image.alt);
    });
  });
});
