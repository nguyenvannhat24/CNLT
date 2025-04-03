document.addEventListener("DOMContentLoaded", function () {
    document.querySelector("form").addEventListener("submit", function (event) {
        event.preventDefault(); // Ngăn chặn gửi form mặc định

        let isValid = true;
        let name = document.getElementById("name");
        let phoneNumber = document.getElementById("phoneNumber");
        let email = document.getElementById("Email");
        let password = document.getElementById("Password");
        let confirmPassword = document.getElementById("comfimPassWord");

        // Xóa thông báo lỗi trước đó
        document.querySelectorAll(".error-message").forEach(e => e.remove());

        // Kiểm tra tên
        if (name.value.trim() === "") {
            showError(name, "Vui lòng nhập tên");
            isValid = false;
        }

        // Kiểm tra số điện thoại (chỉ chấp nhận số, 10 chữ số)
        let phoneRegex = /^[0-9]{10}$/;
        if (!phoneRegex.test(phoneNumber.value)) {
            showError(phoneNumber, "Số điện thoại không hợp lệ (10 chữ số)");
            isValid = false;
        }

        // Kiểm tra email hợp lệ
        let emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
        if (!emailRegex.test(email.value)) {
            showError(email, "Email không hợp lệ");
            isValid = false;
        }

        // Kiểm tra mật khẩu (ít nhất 6 ký tự)
        if (password.value.length < 6) {
            showError(password, "Mật khẩu phải có ít nhất 6 ký tự");
            isValid = false;
        }

        // Kiểm tra nhập lại mật khẩu
        if (password.value !== confirmPassword.value) {
            showError(confirmPassword, "Mật khẩu nhập lại không khớp");
            isValid = false;
        }

        if (isValid) {
            window.location.href = "/Account/Registercomfirm";
 // Chuyển hướng khi hợp lệ
        }
    });

    function showError(inputElement, message) {
        let errorMsg = document.createElement("p");
        errorMsg.className = "error-message";
        errorMsg.style.color = "red";
        errorMsg.style.fontSize = "12px";
        errorMsg.innerText = message;
        inputElement.parentNode.appendChild(errorMsg);
    }
});
