import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Task, TaskResponseDto } from '../../tasks/models/task';
import { Constants } from '../constants';
import { QueryParams } from '../models/query-params.interface';
import { QueryService } from './query.service';
import { AssetResponseDto } from '../models/asset.interface';
import { AuthenticationService } from './authentication.service';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly apiUrl: string = Constants.TASKS;

  constructor(
    private readonly http: HttpClient,
    private readonly queryService: QueryService,
    private readonly authService: AuthenticationService
  ) { }

  private getUserId(): string {
    let userId = '';
    this.authService.currentUser$.subscribe(user => {
      if (user) {
        userId = user.id.toString();
      }
    });
    return userId;
  }

  getTasks(queryParams: QueryParams): Observable<TaskResponseDto> {
    const params = this.queryService.getHttpParams(queryParams);
    return this.http.get<TaskResponseDto>(this.apiUrl, { params });
  }

  searchTasks(queryParams: QueryParams): Observable<TaskResponseDto> {
    let params = this.queryService.getHttpParams(queryParams);
    if (queryParams.ids?.length) {
      queryParams.ids.forEach(id => {
        params = params.append('Ids', id.toString());
      });
    }
    return this.http.get<TaskResponseDto>(`${this.apiUrl}/search`, { params });
  }

  getTask(id: number): Observable<Task> {
    return this.http.get<Task>(`${this.apiUrl}/${id}`);
  }

  createTask(task: Partial<Task>): Observable<Task> {
    return this.http.post<Task>(this.apiUrl, task);
  }

  updateTask(id: number, task: Partial<Task>): Observable<Task> {
    return this.http.put<Task>(`${this.apiUrl}/${id}`, task);
  }

  deleteTask(id: number): Observable<boolean> {
    return this.http.delete<boolean>(`${this.apiUrl}/${id}`);
  }
  
  assignToMe(taskId: number, assigneeId: string): Observable<Task> {
    return this.http.post<Task>(`${this.apiUrl}/${taskId}/assign`, { assigneeId });
  }
  
  completeTask(taskId: number): Observable<Task> {
    return this.http.post<Task>(`${this.apiUrl}/${taskId}/complete`, { completed: true });
  }

  getTaskAssets(taskId: number, queryParams: QueryParams): Observable<AssetResponseDto> {
    const params = this.queryService.getHttpParams(queryParams);
    return this.http.get<AssetResponseDto>(`${this.apiUrl}/${taskId}/assets`, { params });
  }

  searchTasksByTerm(searchTerm: string): Observable<TaskResponseDto> {
    if (!searchTerm || searchTerm.length < 3) {
      const params = new HttpParams()
        .set('page', '1')
        .set('pageSize', '10');
        
      return this.http.get<TaskResponseDto>(this.apiUrl, { params });
    }

    const params = new HttpParams()
      .set('searchTerm', searchTerm);

    return this.http.get<TaskResponseDto>(`${this.apiUrl}/search`, { params });
  }

  getPriorityTasks(): Observable<TaskResponseDto> {
    const params = new HttpParams()
      .set('page', '1')
      .set('pageSize', '5')
      .set('sortColumn', 'priority')
      .set('sortDirection', 'desc')
      .set('status', 'Active')
      .set('userId', this.getUserId());

    return this.http.get<TaskResponseDto>(`${this.apiUrl}/priority`, { params });
  }
}
