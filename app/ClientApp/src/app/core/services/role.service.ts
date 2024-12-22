import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {Constants} from '../constants';
import {RoleResponseDto} from "../models/role.interface";

@Injectable({
    providedIn: 'root'
})
export class RoleService {
    private readonly apiUrl: string = Constants.ROLES;

    constructor(private readonly http: HttpClient) {
    }

    // TODO FIXME this will be a problem if we have more than 1000 roles
    // or if we use searchTerms
    public getAllRoles(): Observable<RoleResponseDto> {
        const params = new HttpParams()
            .set('page', '1')
            .set('pageSize', '1000')
            .set('searchTerm', '')
            .set('sortColumn', '')
            .set('sortDirection', '');
        return this.http.get<RoleResponseDto>(this.apiUrl, {params});
    }
}
