class CourseEnrollment {
    constructor() {
        this.courseId = this.getCourseId();
        this.init();
    }

    init() {
        // Kiểm tra trạng thái đăng ký khi load trang
        this.checkEnrollmentStatus();

        // Gắn sự kiện cho nút đăng ký
        const enrollBtn = document.querySelector('.btn-primary');
        if (enrollBtn) {
            enrollBtn.addEventListener('click', () => this.handleEnroll());
        }

        // Gắn sự kiện cho nút wishlist
        const wishlistBtn = document.querySelector('.btn-secondary');
        if (wishlistBtn) {
            wishlistBtn.addEventListener('click', () => this.handleWishlist());
        }
    }

    async checkEnrollmentStatus() {
        if (!this.courseId) return;

        try {
            const response = await fetch(`/api/Enrollment/Check/${this.courseId}`, {
                headers: this.getHeaders()
            });

            if (response.ok) {
                const data = await response.json();
                if (data.isEnrolled) {
                    this.updateUIForEnrolled(data);
                }
            }
        } catch (error) {
            console.log('Error checking enrollment:', error);
        }
    }

    async handleEnroll() {
        // 1. Kiểm tra đăng nhập
        if (!this.isLoggedIn()) {
            this.showLoginModal();
            return;
        }

        // 2. Lấy thông tin khóa học
        const price = this.getPrice();

        // 3. Confirm trước khi đăng ký
        const result = await Swal.fire({
            title: 'Xác nhận đăng ký',
            html: `
                <p>Bạn có chắc muốn đăng ký khóa học này?</p>
                <div style="margin-top: 15px;">
                    <strong>Học phí: $${price}</strong>
                </div>
            `,
            icon: 'question',
            showCancelButton: true,
            confirmButtonText: 'Đăng ký ngay',
            cancelButtonText: 'Hủy',
            confirmButtonColor: '#667eea',
            cancelButtonColor: '#6c757d'
        });

        if (!result.isConfirmed) return;

        // 4. Hiển thị loading
        this.showLoading();

        try {
            const response = await fetch('/api/Enrollment/Enroll', {
                method: 'POST',
                headers: this.getHeaders(true),
                body: JSON.stringify({
                    courseId: this.courseId,
                    amount: price
                })
            });

            const data = await response.json();

            if (response.ok && data.success) {
                // Đăng ký thành công
                await Swal.fire({
                    title: 'Thành công!',
                    html: `
                        <p>${data.message}</p>
                        <p style="margin-top: 10px;">Chuyển đến trang thanh toán...</p>
                    `,
                    icon: 'success',
                    timer: 2000,
                    showConfirmButton: false
                });

                // Chuyển đến trang thanh toán
                setTimeout(() => {
                    window.location.href = data.redirectUrl;
                }, 2000);
            } else {
                // Xử lý lỗi
                this.showError(data.message || 'Đăng ký thất bại');
            }
        } catch (error) {
            console.error('Enrollment error:', error);
            this.showError('Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại!');
        } finally {
            this.hideLoading();
        }
    }

    isLoggedIn() {
        return !!this.getToken();
    }

    getToken() {
        return localStorage.getItem('authToken') ||
            sessionStorage.getItem('authToken') ||
            this.getCookie('authToken');
    }

    getCookie(name) {
        const value = `; ${document.cookie}`;
        const parts = value.split(`; ${name}=`);
        if (parts.length === 2) return parts.pop().split(';').shift();
        return null;
    }

    getHeaders(includeContentType = false) {
        const headers = {};
        const token = this.getToken();

        if (token) {
            headers['Authorization'] = `Bearer ${token}`;
        }

        if (includeContentType) {
            headers['Content-Type'] = 'application/json';
        }

        return headers;
    }

    getCourseId() {
        // Lấy từ data attribute
        const element = document.querySelector('[data-course-id]');
        if (element) {
            return parseInt(element.dataset.courseId);
        }

        // Hoặc từ URL
        const urlParams = new URLSearchParams(window.location.search);
        const id = urlParams.get('id');
        return id ? parseInt(id) : null;
    }

    getPrice() {
        const priceElement = document.querySelector('.current-price');
        if (!priceElement) return 0;

        const priceText = priceElement.textContent.trim();
        // Loại bỏ ký tự $, dấu phấy và chuyển thành số
        return parseFloat(priceText.replace(/[$,]/g, '')) || 0;
    }

    showLoginModal() {
        Swal.fire({
            title: 'Yêu cầu đăng nhập',
            text: 'Bạn cần đăng nhập để đăng ký khóa học này',
            icon: 'info',
            showCancelButton: true,
            confirmButtonText: 'Đăng nhập',
            cancelButtonText: 'Hủy',
            confirmButtonColor: '#667eea',
            cancelButtonColor: '#6c757d'
        }).then((result) => {
            if (result.isConfirmed) {
                // Lưu URL hiện tại để redirect về sau khi đăng nhập
                sessionStorage.setItem('redirectAfterLogin', window.location.href);
                window.location.href = '/Account/Login';
            }
        });
    }

    showLoading() {
        const btn = document.querySelector('.btn-primary');
        if (!btn) return;

        btn.disabled = true;
        btn.dataset.originalText = btn.innerHTML;
        btn.innerHTML = '<i class="bi bi-hourglass-split"></i> Đang xử lý...';
    }

    hideLoading() {
        const btn = document.querySelector('.btn-primary');
        if (!btn) return;

        btn.disabled = false;
        if (btn.dataset.originalText) {
            btn.innerHTML = btn.dataset.originalText;
        }
    }

    showSuccess(message) {
        Swal.fire({
            title: 'Thành công!',
            text: message,
            icon: 'success',
            confirmButtonColor: '#667eea'
        });
    }

    showError(message) {
        Swal.fire({
            title: 'Lỗi!',
            text: message,
            icon: 'error',
            confirmButtonColor: '#667eea'
        });
    }

    updateUIForEnrolled(data) {
        const btn = document.querySelector('.btn-primary');
        if (!btn) return;

        if (data.isPaid) {
            // Đã thanh toán - cho phép học
            btn.textContent = 'Tiếp tục học';
            btn.classList.add('enrolled-btn');
            btn.onclick = (e) => {
                e.preventDefault();
                window.location.href = `/Course/Learning?enrollmentId=${data.enrollmentId}`;
            };
        } else {
            // Chưa thanh toán - chuyển đến trang thanh toán
            btn.textContent = 'Thanh toán';
            btn.classList.add('payment-btn');
            btn.onclick = (e) => {
                e.preventDefault();
                window.location.href = `/Payment/Checkout?enrollmentId=${data.enrollmentId}`;
            };
        }

        // Thêm badge đã đăng ký
        this.addEnrolledBadge(data);
    }

    addEnrolledBadge(data) {
        const card = document.querySelector('.enrollment-card');
        if (!card || card.querySelector('.enrolled-badge')) return;

        const badge = document.createElement('div');
        badge.className = 'enrolled-badge';

        if (data.isPaid) {
            badge.innerHTML = `
                <i class="bi bi-check-circle-fill"></i> 
                Đã đăng ký - Tiến độ: ${data.progress}%
            `;
            badge.style.background = 'linear-gradient(135deg, #11998e 0%, #38ef7d 100%)';
        } else {
            badge.innerHTML = '<i class="bi bi-exclamation-circle-fill"></i> Chưa thanh toán';
            badge.style.background = 'linear-gradient(135deg, #f093fb 0%, #f5576c 100%)';
        }

        card.insertBefore(badge, card.firstChild);
    }

    async handleWishlist() {
        if (!this.isLoggedIn()) {
            this.showLoginModal();
            return;
        }

        // TODO: Implement wishlist functionality
        this.showSuccess('Đã thêm vào danh sách yêu thích!');
    }
}

// Khởi tạo khi DOM ready
document.addEventListener('DOMContentLoaded', () => {
    new CourseEnrollment();
});

// Xử lý redirect sau khi đăng nhập
window.addEventListener('load', () => {
    const redirectUrl = sessionStorage.getItem('redirectAfterLogin');
    if (redirectUrl) {
        sessionStorage.removeItem('redirectAfterLogin');
        // Refresh enrollment status
        if (window.location.href === redirectUrl) {
            location.reload();
        }
    }
});