import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Task, TaskResponseDto } from '../../tasks/models/task';
import { Constants } from '../constants';
import { QueryParams } from '../models/query-params.interface';
import { QueryService } from './query.service';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private readonly apiUrl: string = Constants.TASKS;

  constructor(
    private readonly http: HttpClient,
    private readonly queryService: QueryService
  ) { }

  getTasks(queryParams: QueryParams): Observable<TaskResponseDto> {
    const params = this.queryService.getHttpParams(queryParams);

    return this.http.get<TaskResponseDto>(this.apiUrl, { params });
  }

  searchTasks(queryParams: QueryParams): Observable<TaskResponseDto> {
    const params = this.queryService.getHttpParams(queryParams);
    
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
}
