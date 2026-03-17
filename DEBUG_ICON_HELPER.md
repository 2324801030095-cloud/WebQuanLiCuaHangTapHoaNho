## 🔍 HƯỚNG DẪN DEBUG - KIỂM TRA ICON DANH MỤC

### ✅ Cách 1: Kiểm tra Debug Output (Recommended)

1. **Mở Visual Studio Output Window:**
   - Nhấn `Ctrl+Alt+O` hoặc `View → Output`
   - Chọn dropdown "Debug" hoặc "Application"

2. **Chạy ứng dụng (F5)**

3. **Truy cập trang Home hoặc danh mục, xem Output window**
   - Nếu tên danh mục KHÔNG khớp, sẽ thấy log:
   ```
   [CategoryIconHelper] Icon không tìm thấy cho: 'Rau Cua Qua' (trimmed: 'Rau Cua Qua', unaccented: 'Rau Cua Qua')
   ```
   - Nếu khớp, KHÔNG có log, icon sẽ hiển thị đúng

---

### ✅ Cách 2: Kiểm tra trực tiếp bằng Inspect Element

1. **Mở trang Home hoặc DanhMuc/DanhSach**
2. **Nhấn F12 để mở Developer Tools**
3. **Tìm phần menu danh mục, inspect một item:**
   ```html
   <!-- Nếu icon đúng, sẽ thấy: -->
   <i class="bi bi-leaf me-2 text-success"></i>
   
   <!-- Nếu fallback, sẽ thấy: -->
   <i class="bi bi-tag me-2 text-success"></i>
   ```

---

### ✅ Cách 3: Kiểm tra Dữ liệu Database

**Chạy query này để xem TenDM thực tế:**
```sql
SELECT [MaDM], [TenDM] FROM [QuanLyTapHoaThanhNhan].[dbo].[DanhMuc]
ORDER BY [MaDM]
```

**So sánh với keys trong CategoryIconHelper.cs:**
```csharp
["Rau Củ Quả"] = "bi-leaf",        // ← Key phải KHỚP TUYỆT ĐỐI với DB
["Đồ Uống"] = "bi-cup-straw",
["Bánh Kẹo"] = "bi-cake2",
// ...
```

**Nếu DB có dấu nhưng helper dùng không dấu → KHÔNG khớp → fallback icon**

---

### 🔧 Nếu Icon Vẫn Không Hiển Thị Đúng:

**Bước 1: Xem log debug**
- Lưu ý tên danh mục chính xác từ log
- Ví dụ: `'Rau Cua Qua'` hoặc `'Rau Cũ Quả'`

**Bước 2: Cập nhật helper với tên chính xác**
```csharp
// File: Helpers/CategoryIconHelper.cs
private static readonly Dictionary<string, string> CategoryIconMap =
    new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // Thêm tên chính xác từ DB vào đây
        ["Tên_Chính_Xác_Từ_DB"] = "bi-icon",
    };
```

**Bước 3: Rebuild + Refresh browser (Ctrl+F5)**

---

### 📊 Mapping Icons Hiện Tại:

| TenDM (DB) | Icon | Visual |
|---|---|---|
| Rau Củ Quả | bi-leaf | 🌿 Lá |
| Đồ Uống | bi-cup-straw | 🥤 Cốc |
| Bánh Kẹo | bi-cake2 | 🍰 Bánh |
| Sữa Các Loại | bi-cup | 🥛 Cốc |
| Gia Vị Và Dầu Ăn | bi-droplet | 💧 Giọt |
| Hóa Phẩm Gia Dụng | bi-bucket | 🪣 Thùng |
| Gạo Và Ngũ Cốc | bi-grain | 🌾 Lúa |
| Thịt Và Trứng | bi-egg | 🥚 Trứng |

---

### 🎯 Kết Luận

Helper `CategoryIconHelper.cs` đã được:
✅ Tạo tại `Helpers/CategoryIconHelper.cs`
✅ Integration vào `Views/DanhMuc/Menu.cshtml`
✅ Integration vào `Views/Shared/_MenuDanhMuc.cshtml`
✅ Integration vào `Views/DanhMuc/DanhSach.cshtml`

Nếu icon vẫn là `bi-tag` cho tất cả, hãy:
1. Kiểm tra Output Window debug log
2. Xem tên TenDM chính xác từ DB
3. Thêm vào dictionary với tên chính xác
4. Rebuild + Clear browser cache (Ctrl+F5)
