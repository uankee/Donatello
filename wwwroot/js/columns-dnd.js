(function () {
    const container = document.getElementById('columnsContainer');
    if (!container) return;

    let dragEl = null;
    let placeholder = null;

    function createPlaceholder(width, height) {
        const ph = document.createElement('div');
        ph.className = 'column-placeholder';
        ph.style.width = width + 'px';
        ph.style.minHeight = height + 'px';
        ph.style.border = '2px dashed rgba(0,0,0,0.12)';
        ph.style.borderRadius = '6px';
        ph.style.background = 'rgba(0,0,0,0.02)';
        return ph;
    }

    function onDragStart(e) {
        dragEl = this;
        e.dataTransfer.effectAllowed = 'move';
        e.dataTransfer.setData('text/plain', this.dataset.columnId);

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
        const target = e.target.closest('.column-card');
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
        const ids = Array.from(container.querySelectorAll('.column-card'))
            .map(el => parseInt(el.dataset.columnId, 10));

        const tokenInput = document.querySelector('#antiForgeryForm input[name="__RequestVerificationToken"]');
        const token = tokenInput ? tokenInput.value : '';

        const payload = {
            boardId: window.__currentBoardId || null,
            columnIds: ids
        };

        fetch('/Columns/Reorder', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': token
            },
            credentials: 'same-origin',
            body: JSON.stringify(payload)
        }).then(resp => {
            if (!resp.ok) {
                console.error('Failed to save columns order', resp.statusText);
            }
        }).catch(err => {
            console.error('Error saving columns order', err);
        });
    }

    function attach() {
        const items = container.querySelectorAll('.column-card');
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
