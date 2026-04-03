import axios from 'axios';

const API = axios.create({
    baseURL: 'http://localhost:5254/api',
    headers: {
        'Content-Type': 'application/json',
    }
});

// Add token to requests
API.interceptors.request.use(
    (config) => {
        const token = localStorage.getItem('token');
        if (token) {
            config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
    },
    (error) => Promise.reject(error)
);

// Response interceptor for error handling
API.interceptors.response.use(
    (response) => response,
    (error) => {
        if (error.response?.status === 401) {
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            window.location.href = '/login';
        }
        return Promise.reject(error);
    }
);

export const authService = {
    register: (userData) => API.post('/auth/register', userData),
    login: (credentials) => API.post('/auth/login', credentials),
    getProfile: () => API.get('/auth/profile'),
    forgotPassword: (email) => API.post('/auth/forgot-password', { email }),
    verifyOtp: (email, otpCode) => API.post('/auth/verify-otp', { email, otpCode }),
    resetPassword: (data) => API.post('/auth/reset-password', data),
};

export default API;