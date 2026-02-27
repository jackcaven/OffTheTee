import axios from 'axios';
import type { ApiResponse } from '../types/api';

export const apiClient = axios.create({
  baseURL: '/api/v1',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Attach JWT token to every request
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('accessToken');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Unwrap the ApiResponse envelope
apiClient.interceptors.response.use(
  (response) => {
    const body = response.data as ApiResponse<unknown>;
    if (body.error) {
      return Promise.reject(new Error(body.error));
    }
    response.data = body.data;
    return response;
  },
  (error) => {
    if (axios.isAxiosError(error) && error.response) {
      const body = error.response.data as ApiResponse<unknown>;
      const message = body?.error ?? error.message;
      return Promise.reject(new Error(message));
    }
    return Promise.reject(error);
  }
);

export default apiClient;
