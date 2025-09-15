(function () {
    const container = document.getElementById('boardsContainer');
    if (!container) return;

    let dragEl = null;
    let placeholder = null;

    function createPlaceholder(width, height) {
        const ph = document.createElement('div');
        ph.className = 'board-placeholder';
        ph.style.minWidth = width + 'px';
        ph.style.minHeight = height + 'px';
        ph.style.border = '2px dashed rgba(0,0,0,0.15)';
        ph.style.borderRadius = '6px';
        ph.style.background = 'rgba(0,0,0,0.02)';
        return ph;
    }

    function onDragStart(e) {
        dragEl = this;
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/plain', this.dataset.boardId);

        this.classList.add('dragging');
        placeholder = createPlaceholder(this.offsetWidth, this.offsetHeight);
        this.parentNode.insertBefore(placeholder, this.nextSibling);

        setTimeout(() => this.style.visibility = 'hidden', 0);
    }

    function onDragEnd(e) {
        if (!dragEl) return;
        dragEl.style.visibility = '';
        dragEl.classList.remove('dragging');
        if (placeholder && placeholder.parentNode) placeholder.parentNode.removeChild(placeholder);
        placeholder = null;
        dragEl = null;

        saveOrder();
    }

    function onDragOver(e) {
        e.preventDefault(); 
        e.dataTransfer.dropEffect = 'move';
        const target = e.target.closest('.board-card');
        if (!target || target === dragEl) return;

        const rect = target.getBoundingClientRect();
        const after = (e.clientX - rect.left) > rect.width / 2;
        if (after) {
            target.parentNode.insertBefore(placeholder, target.nextSibling);
        } else {
            target.parentNode.insertBefore(placeholder, target);
        }
    }

    function onDrop(e) {
        e.preventDefault();
        if (!dragEl || !placeholder) return;
        placeholder.parentNode.insertBefore(dragEl, placeholder);
    }

    function saveOrder() {
        const ids = Array.from(container.querySelectorAll('.board-card'))
            .map(el => parseInt(el.dataset.boardId, 10));

        const tokenInput = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : null;

        fetch('/Boards/Reorder', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token || ''
            },
            body: JSON.stringify(ids),
            credentials: 'same-origin'
        }).then(resp => {
            if (!resp.ok) {
                console.error('Failed to save order', resp.statusText);
            }
        }).catch(err => {
            console.error('Error saving board order', err);
        });
    }

    function attach() {
        const items = container.querySelectorAll('.board-card');
        items.forEach(it => {
            it.setAttribute('draggable', 'true');
            it.addEventListener('dragstart', onDragStart);
            it.addEventListener('dragend', onDragEnd);
        });

        container.addEventListener('dragover', onDragOver);
        container.addEventListener('drop', onDrop);
    }

    attach();

})();
