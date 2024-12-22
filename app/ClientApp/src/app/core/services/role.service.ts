import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Constants} from '../constants';
import {RoleResponse} from "../models/role.interface";

@Injectable({
    providedIn: 'root'
})
export class RoleService {
    private readonly apiUrl: string = Constants.ROLES;

    constructor(private readonly http: HttpClient) {
    }

    public getAllRoles(): Observable<RoleResponse> {
        const params = new HttpParams()
            .set('page', '1')
            .set('pageSize', '100')
            .set('searchTerm', '')
            .set('sortColumn', '')
            .set('sortDirection', '');
        return this.http.get<RoleResponse>(this.apiUrl, {params});
    }
}
