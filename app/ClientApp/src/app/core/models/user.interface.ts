export interface User {
  id: number;
  username: string;
  email: string;
  createdAt: Date;
  roles?: string[];
}

export interface UserResponse {
  items: User[];
  total: number;
  page: number;
  pageSize: number;
}

export interface UserQueryParams {
  page: number;
  pageSize: number;
  searchTerm?: string;
  sortColumn?: string;
  sortDirection?: 'asc' | 'desc';
}
