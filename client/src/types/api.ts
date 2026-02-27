export interface ApiResponse<T> {
  data: T | null;
  error: string | null;
}

export interface AuthResult {
  accessToken: string;
  expiresAt: string;
  userId: string;
  email: string;
  displayName: string;
}
