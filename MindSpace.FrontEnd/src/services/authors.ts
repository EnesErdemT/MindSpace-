import api from './api';

export interface Author {
  id: string;
  userName: string;
  email: string;
  firstName: string;
  lastName: string;
  bio: string | null;
  profileImageUrl: string | null;
  followerCount: number;
  followingCount: number;
  isVerified: boolean;
  roles: string[];
}

export interface AuthorsResponse {
  users: Author[];
  totalCount: number;
}

export const authorsService = {
  async getAuthors(page = 1, pageSize = 10): Promise<AuthorsResponse> {
    const response = await api.get('/Users', { params: { page, pageSize } });
    return {
      users: response.data.users,
      totalCount: response.data.totalCount
    };
  },

  async getAuthorById(userName: string): Promise<Author> {
    const response = await api.get(`/Users/${userName}`);
    return response.data;
  }
}; 