import api from './api'
import { AuthResponse, LoginRequest, RegisterRequest, User } from '@/types'
import Cookies from 'js-cookie'

export const authService = {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/login', credentials)
    
    if (response.data.success && response.data.token) {
      Cookies.set('authToken', response.data.token, { expires: 7 })
      if (response.data.refreshToken) {
        Cookies.set('refreshToken', response.data.refreshToken, { expires: 30 })
      }
    }
    
    return response.data
  },

  async register(userData: RegisterRequest): Promise<AuthResponse> {
    const response = await api.post<AuthResponse>('/auth/register', userData)
    
    if (response.data.success && response.data.token) {
      Cookies.set('authToken', response.data.token, { expires: 7 })
      if (response.data.refreshToken) {
        Cookies.set('refreshToken', response.data.refreshToken, { expires: 30 })
      }
    }
    
    return response.data
  },

  async logout(): Promise<void> {
    try {
      await api.post('/auth/logout')
    } catch (error) {
      console.error('Logout error:', error)
    } finally {
      Cookies.remove('authToken')
      Cookies.remove('refreshToken')
    }
  },

  async getCurrentUser(): Promise<User> {
    const response = await api.get<User>('/auth/me')
    return response.data
  },

  async refreshToken(): Promise<AuthResponse> {
    const token = Cookies.get('authToken')
    const refreshToken = Cookies.get('refreshToken')
    
    if (!token || !refreshToken) {
      throw new Error('No tokens available')
    }

    const response = await api.post<AuthResponse>('/auth/refresh-token', null, {
      params: { token, refreshToken }
    })
    
    if (response.data.success && response.data.token) {
      Cookies.set('authToken', response.data.token, { expires: 7 })
      if (response.data.refreshToken) {
        Cookies.set('refreshToken', response.data.refreshToken, { expires: 30 })
      }
    }
    
    return response.data
  },

  isAuthenticated(): boolean {
    return !!Cookies.get('authToken')
  },

  async forgotPassword(email: string): Promise<{ success: boolean; message: string; resetToken?: string }> {
    const response = await api.post('/auth/forgot-password', { email })
    return response.data
  },

  async resetPassword(email: string, token: string, newPassword: string): Promise<{ success: boolean; message: string }> {
    const response = await api.post('/auth/reset-password', { email, token, newPassword })
    return response.data
  },

  async verifyEmail(token: string): Promise<AuthResponse> {
    const response = await api.get<AuthResponse>(`/auth/verify-email?token=${encodeURIComponent(token)}`)
    if (response.data.success && response.data.token) {
      Cookies.set('authToken', response.data.token, { expires: 7 })
      if (response.data.refreshToken) {
        Cookies.set('refreshToken', response.data.refreshToken, { expires: 30 })
      }
    }
    return response.data
  },

  async resendVerification(email: string): Promise<{ success: boolean; message: string }> {
    const response = await api.post('/auth/resend-verification', { email })
    return response.data
  }
}