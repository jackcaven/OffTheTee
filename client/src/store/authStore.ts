import { create } from 'zustand';
import type { AuthResult } from '../types/api';

interface AuthState {
  user: Pick<AuthResult, 'userId' | 'email' | 'displayName'> | null;
  token: string | null;
  setAuth: (result: AuthResult) => void;
  clearAuth: () => void;
  isAuthenticated: boolean;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  token: localStorage.getItem('accessToken'),
  isAuthenticated: !!localStorage.getItem('accessToken'),

  setAuth: (result: AuthResult) => {
    localStorage.setItem('accessToken', result.accessToken);
    set({
      token: result.accessToken,
      isAuthenticated: true,
      user: {
        userId: result.userId,
        email: result.email,
        displayName: result.displayName,
      },
    });
  },

  clearAuth: () => {
    localStorage.removeItem('accessToken');
    set({ token: null, isAuthenticated: false, user: null });
  },
}));
