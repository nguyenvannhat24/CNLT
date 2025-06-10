var items = document.querySelectorAll(".grid-item");

function ChonDanhMuc(n) {
  for (var i = 0; i < items.length; i++) {
    items[i].style.backgroundColor = "transparent";
    var text1 = items[i].querySelector("p");
    if (text1) {
      text1.style.fontWeight = "normal"; // Làm đậm chữ
    }
  }
  items[n].style.backgroundColor = "rgb(138, 226, 138)"; // nền đổi màu
  var text = items[n].querySelector("p");
  if (text) {
    text.style.fontWeight = "bold"; // Làm đậm chữ
  }
}

for (let i = 0; i < items.length; i++) {
  items[i].onclick = function () {
    ChonDanhMuc(i);
  };
}
var items1 = document.querySelectorAll(".product53-item");

function ChonSanPham(n) {
  // Reset viền và màu của tất cả nút "Thêm vào giỏ hàng"
  for (let j = 0; j < items1.length; j++) {
    items1[j].style.border = "2px solid #aca5a5"; // Reset viền
    var btnThemGioHang = items1[j].querySelector("button");
    if (btnThemGioHang) {
      btnThemGioHang.style.backgroundColor = "transparent"; // Reset màu
    }
  }

  // Đổi viền và màu của phần tử được chọn
  items1[n].style.border = "2px solid green"; // Viền xanh để đánh dấu sản phẩm được chọn
  var btnThemGioHang1 = items1[n].querySelector("button");
  if (btnThemGioHang1) {
    btnThemGioHang1.style.backgroundColor = "rgb(71, 175, 71)"; // Màu xanh khi được chọn
  }
}

// Gán sự kiện onclick cho tất cả phần tử
for (let i = 0; i < items1.length; i++) {
  items1[i].onclick = function () {
    ChonSanPham(i);
  };
}
