import {Injectable} from '@angular/core';
import {QueryParams} from "../models/query-params.interface";
import {HttpParams} from "@angular/common/http";

@Injectable({
    providedIn: 'root'
})
export class QueryService {

    constructor() {
    }

    public normaliseQueryParams(params: QueryParams): QueryParams {
        if (!params.page || params.page < 1) {
            params.page = Math.max(params.page, 1);
        }
        if (!params.pageSize || params.pageSize < 1) {
            params.pageSize = Math.max(params.pageSize, 1);
        }
        if (!params.sortColumn || !params.sortColumn.trim()) {
            params.sortColumn = 'id';
        }
        if (!params.sortDirection || (params.sortDirection !== 'asc' && params.sortDirection !== 'desc')) {
            params.sortDirection = 'asc';
        }
        return params
    }

    public getHttpParams(params: QueryParams): HttpParams {
        this.normaliseQueryParams(params);
        let httpParams: HttpParams = new HttpParams()
            .set('page', params.page.toString())
            .set('pageSize', params.pageSize.toString());

        if (params.searchTerm) {
            httpParams = httpParams.set('searchTerm', params.searchTerm);
        }
        if (params.sortColumn) {
            httpParams = httpParams.set('sortColumn', params.sortColumn);
            httpParams = httpParams.set('sortDirection', params.sortDirection || 'asc');
        }
        return httpParams;
    }

    public getQueryParams(): QueryParams {
        return {
            page: 1,
            pageSize: 50,
            searchTerm: '',
            sortColumn: 'id',
            sortDirection: 'asc'
        };
    }
}
