import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {CreateUser, UpdateUser, User, UserResponse, UserResponseDto} from '../models/user.interface';
import {QueryParams} from '../models/query-params.interface';
import {Constants, DefaultValues} from '../constants';
import {QueryService} from "./query.service";

@Injectable({
    providedIn: 'root'
})
export class UserService {
    private readonly apiUrl: string = Constants.USERS;

    constructor(private readonly http: HttpClient, private readonly queryService: QueryService) {
    }

    public getUsers(params: QueryParams): Observable<UserResponseDto<UserResponse>> {
        let httpParams: HttpParams = this.queryService.getHttpParams(params);
        return this.http.get<UserResponseDto<UserResponse>>(this.apiUrl, {params: httpParams});
    }

    public deleteUser(id: number): Observable<void> {
        return this.http.delete<void>(`${this.apiUrl}/${id}`);
    }

    public createUser(user: CreateUser): Observable<User> {
        return this.http.post<User>(this.apiUrl, user);
    }

    public updateUser(user: UpdateUser): Observable<User> {
        return this.http.put<User>(`${this.apiUrl}/${user.id}`, user);
    }

    // TODO FIXME WTF
    // this method SHOULD take in a Map<number, string> but it's taking in an Object instead
    // because the API / dialog is returning an Object
    // so now we have to convert it back to a Map
    // this is a mess
    public formatRolesForSelection(roles: Object): number[] {
        return Object.keys(roles).map(key => parseInt(key));
    }

    public getUsersByIds(userIds: number[]) {
        const params: QueryParams = this.queryService.getQueryParams();
        params.pageSize = Constants.MAX_PAGE_SIZE;
        const httpParams: HttpParams = this.queryService.getHttpParams(params);
        httpParams.set(Constants.ID, userIds.join(','));
        return this.http.get<UserResponseDto<UserResponse>>(this.apiUrl, {params: httpParams});
    }
}
