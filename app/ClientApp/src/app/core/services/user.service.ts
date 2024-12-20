import {Injectable} from '@angular/core';
import {HttpClient, HttpParams} from '@angular/common/http';
import {Observable} from 'rxjs';
import {CreateUser, UpdateUser, User, UserResponse} from '../models/user.interface';
import {UserQueryParams} from '../models/user-query-params.interface';
import {Constants} from '../constants';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl: string = Constants.USERS;

  constructor(private readonly http: HttpClient) {
  }

  public getUsers(params: UserQueryParams): Observable<UserResponse> {
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

    return this.http.get<UserResponse>(this.apiUrl, {params: httpParams});
  }

  public deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  createUser(user: CreateUser): Observable<User> {
    return this.http.post<User>(this.apiUrl, user);
  }

  updateUser(user: UpdateUser): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${user.id}`, user);
  }
}
