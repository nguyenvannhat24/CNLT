// Cài đặt chỉ số cho từng carousel
let slideIndex1 = 1; // Dùng cho carousel 1
let slideIndex2 = 1; // Dùng cho carousel 2
let slidesToShow = 5; // Số lượng sản phẩm hiển thị mỗi lần

// Hiển thị slideshow đầu tiên (carousel 1)
showSlides(slideIndex1, 1);

// Hiển thị slideshow thứ hai (carousel 2)
showSlides(slideIndex2, 2);

// Điều khiển Next/Previous cho cả hai carousel
function plusSlides(n, carouselNumber) {
  if (carouselNumber === 1) {
    showSlides((slideIndex1 += n), carouselNumber);
  } else if (carouselNumber === 2) {
    showSlides((slideIndex2 += n), carouselNumber);
  }
}

// Hiển thị slides cho mỗi carousel riêng biệt
function showSlides(n, carouselNumber) {
  let i;
  let slides;

  // Chọn đúng các slides dựa trên số của carousel
  if (carouselNumber === 1) {
    slides = document.getElementsByClassName("mySlides1");
  } else if (carouselNumber === 2) {
    slides = document.getElementsByClassName("mySlides2");
  }

  // Kiểm tra giới hạn slide và điều chỉnh chỉ số
  if (n > slides.length - slidesToShow + 1) {
    if (carouselNumber === 1) {
      slideIndex1 = 1;
    } else if (carouselNumber === 2) {
      slideIndex2 = 1;
    }
  }
  if (n < 1) {
    if (carouselNumber === 1) {
      slideIndex1 = slides.length - slidesToShow + 1;
    } else if (carouselNumber === 2) {
      slideIndex2 = slides.length - slidesToShow + 1;
    }
  }

  // Ẩn tất cả các slides
  for (i = 0; i < slides.length; i++) {
    slides[i].style.display = "none";
  }

  // Hiển thị 5 slide cho mỗi lần (hoặc số lượng đã xác định trong `slidesToShow`)
  for (
    i = slideIndex1 - 1;
    i < slideIndex1 - 1 + slidesToShow && i < slides.length;
    i++
  ) {
    if (carouselNumber === 1) {
      slides[i].style.display = "block";
    }
  }

  // Tương tự cho carousel 2
  for (
    i = slideIndex2 - 1;
    i < slideIndex2 - 1 + slidesToShow && i < slides.length;
    i++
  ) {
    if (carouselNumber === 2) {
      slides[i].style.display = "block";
    }
  }
}
