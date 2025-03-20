import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { Task, TaskResponseDto } from './models/task';

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  private apiUrl = 'api/v1/tasks';

  constructor(private http: HttpClient) { }

  getTasks(page: number = 1, pageSize: number = 10): Observable<TaskResponseDto> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<TaskResponseDto>(this.apiUrl, { params });
  }

  getTasksForEntity(entityType: string, entityId: number, page: number = 1, pageSize: number = 10): Observable<TaskResponseDto> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('entityType', entityType)
      .set('entityId', entityId.toString());

    return this.http.get<TaskResponseDto>(this.apiUrl, { params });
  }

  getProgramTasks(programId: number, page: number = 1, pageSize: number = 10): Observable<TaskResponseDto> {
    return this.getTasksForEntity('Program', programId, page, pageSize);
  }

  getProjectTasks(projectId: number, page: number = 1, pageSize: number = 10): Observable<TaskResponseDto> {
    return this.getTasksForEntity('Project', projectId, page, pageSize);
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
