export interface QueryParams {
    page: number;
    pageSize: number;
    searchTerm?: string;
    sortColumn?: string;
    sortDirection?: 'asc' | 'desc';
    status?: string;
    priority?: string;
    [key: string]: any;
}
