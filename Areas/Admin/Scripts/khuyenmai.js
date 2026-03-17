// lightweight helper for admin khuyen mai interactions
document.addEventListener('click', function (e) {
    const addBtn = e.target.closest('#btnAddKm');
    const editBtn = e.target.closest('.btn-edit-km');
    const delBtn = e.target.closest('.btn-del-km');
    if (addBtn) {
        fetch('/Admin/KhuyenMai/Them').then(r => r.text()).then(html => {
            document.getElementById('modalArea').innerHTML = html;
            const modalEl = document.querySelector('#modalKM');
            if (modalEl) new bootstrap.Modal(modalEl).show();
        });
    }
    if (editBtn) {
        fetch(`/Admin/KhuyenMai/Sua/${editBtn.dataset.id}`).then(r => r.text()).then(html => {
            document.getElementById('modalArea').innerHTML = html;
            const modalEl = document.querySelector('#modalKM');
            if (modalEl) new bootstrap.Modal(modalEl).show();
        });
    }
    if (delBtn) {
        if (!confirm('Xóa khuyến mãi?')) return;
        const id = parseInt(delBtn.dataset.id);
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        const formData = new FormData();
        if (token) formData.append('__RequestVerificationToken', token);
        formData.append('id', id);
        fetch('/Admin/KhuyenMai/Xoa', { method: 'POST', body: formData }).then(r => r.json()).then(d => {
            alert(d.message);
            if (d.success) location.reload();
        });
    }
});
