import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { Subject, catchError, of, merge, Observable } from 'rxjs';
import { filter, startWith, switchMap, debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Router } from '@angular/router';
import { Task, TaskResponseDto } from '../models/task';
import { TaskService } from '../../core/services/task.service';
import { QueryService } from '../../core/services/query.service';
import { Constants } from 'src/app/core/constants';
import { QueryParams } from '../../core/models/query-params.interface';
import { NotificationUtils } from '../../core/utils/notification.utils';
import { ConfirmDialogComponent } from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';
import { BaseDialogComponent, DialogField } from 'src/app/shared/components/base-dialog/base-dialog.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { AuthenticationService } from '../../core/services/authentication.service';
import { AuthorisationService } from '../../core/services/authorisation.service';

@Component({
  selector: 'app-task-list',
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit, AfterViewInit {
  public readonly displayedColumns: string[] = ['id', 'name', 'status', 'assignee', 'priority', 'dueDate', 'completionPercentage', 'actions'];
  public readonly dataSource: MatTableDataSource<Task> = new MatTableDataSource<Task>([]);
  public searchTerm: Subject<string> = new Subject<string>();
  public isLoading: boolean = false;
  public totalCount: number = 0;
  public currentSearchValue: string = '';
  public errorMessage: string = '';
  public readonly defaultPageSize: number = Constants.DEFAULT_PAGE_SIZE;
  public filters: { [key: string]: string } = {};
  public currentUserId: string = '';
  public isAdmin: boolean = false;

  @ViewChild(MatPaginator) public paginator!: MatPaginator;
  @ViewChild(MatSort) private readonly sort!: MatSort;

  constructor(
    private readonly taskService: TaskService,
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly queryService: QueryService,
    private readonly snackBar: MatSnackBar,
    private readonly authService: AuthenticationService,
    private readonly authorisationService: AuthorisationService
  ) { }

  ngOnInit(): void {
    this.isLoading = true;
    this.authService.currentUser$.subscribe(user => {
      this.currentUserId = user?.id || '';
      this.loadTasks();
    });
    
    this.authorisationService.isAdmin$.subscribe(isAdmin => {
      this.isAdmin = isAdmin;
    });
    
    const params = this.getDefaultQueryParams();
    this.fetchData(params);
  }

  ngAfterViewInit(): void {
    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
    
    if (this.paginator) {
      this.setupSearchAndSort();
    }
  }

  public loadTasks(): void {
    if (this.paginator && this.sort) {
      const params = this.buildQueryParams();
      this.fetchData(params);
    }
  }

  public onSearch(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.currentSearchValue = target.value;
    this.searchTerm.next(this.currentSearchValue);
  }

  public navigateToTaskDetail(taskId: number): void {
    this.router.navigate(['/tasks/detail', taskId]);
  }

  public canCompleteTask(task: Task): boolean {
    return this.isAdmin || (task.assigneeId !== null && task.assigneeId?.toString() === this.currentUserId);
  }

  public isAssignedToMe(task: Task): boolean {
    if (!this.currentUserId || task.assigneeId === undefined || task.assigneeId === null) {
      return false;
    }
    
    const currentUserIdNumber = Number(this.currentUserId);
    return task.assigneeId === currentUserIdNumber;
  }

  public assignToMe(task: Task, event: Event): void {
    if (event) {
      event.stopPropagation();
    }
    
    console.log('Assigning task to user with ID:', this.currentUserId, 'typeof:', typeof this.currentUserId);
    console.log('Task assigneeId before:', task.assigneeId, 'typeof:', typeof task.assigneeId);
    
    this.taskService.assignToMe(task.id, this.currentUserId)
      .pipe(
        catchError(error => {
          this.snackBar.open('Failed to assign task', 'Close', {
            duration: 3000
          });
          return of(null);
        })
      )
      .subscribe(updatedTask => {
        if (updatedTask) {
          console.log('Task assigneeId after:', updatedTask.assigneeId, 'typeof:', typeof updatedTask.assigneeId);
          this.loadTasks();
          this.snackBar.open('Task assigned successfully', 'Close', {
            duration: 3000
          });
        }
      });
  }

  public completeTask(task: Task, event: Event): void {
    if (event) {
      event.stopPropagation();
    }
    
    if (!this.canCompleteTask(task)) {
      this.snackBar.open('You are not authorized to complete this task', 'Close', {
        duration: 3000
      });
      return;
    }
    
    this.taskService.completeTask(task.id)
      .pipe(
        catchError(error => {
          this.snackBar.open('Failed to complete task', 'Close', {
            duration: 3000
          });
          return of(null);
        })
      )
      .subscribe(updatedTask => {
        if (updatedTask) {
          this.loadTasks();
          this.snackBar.open('Task completed successfully', 'Close', {
            duration: 3000
          });
        }
      });
  }

  public editTask(task: Task, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }

    const editableFields: DialogField[] = [
      { name: 'name', type: 'text', label: 'Name', required: true },
      { name: 'description', type: 'text', label: 'Description', required: false },
      { 
        name: 'status', 
        type: 'select', 
        label: 'Status', 
        required: true,
        options: [
          { value: 'New', display: 'New' },
          { value: 'In Progress', display: 'In Progress' },
          { value: 'On Hold', display: 'On Hold' },
          { value: 'Completed', display: 'Completed' },
          { value: 'Cancelled', display: 'Cancelled' }
        ]
      },
      { 
        name: 'priority', 
        type: 'select', 
        label: 'Priority', 
        required: true,
        options: [
          { value: 'Low', display: 'Low' },
          { value: 'Medium', display: 'Medium' },
          { value: 'High', display: 'High' },
          { value: 'Critical', display: 'Critical' }
        ]
      },
      { name: 'startDate', type: 'date', label: 'Start Date', required: false },
      { name: 'dueDate', type: 'date', label: 'Due Date', required: false },
      { name: 'completionPercentage', type: 'text', label: 'Completion %', required: false },
      { name: 'estimatedHours', type: 'text', label: 'Estimated Hours', required: false },
      { name: 'actualHours', type: 'text', label: 'Actual Hours', required: false },
      { name: 'assigneeId', type: 'text', label: 'Assignee ID', required: false },
      { name: 'assigneeName', type: 'text', label: 'Assignee Name', required: false },
      { name: 'projectId', type: 'text', label: 'Project ID', required: false },
      { name: 'parentTaskId', type: 'text', label: 'Parent Task ID', required: false }
    ];

    const dialogRef = this.dialog.open(BaseDialogComponent, {
      width: Constants.DEFAULT_DIALOG_WIDTH,
      data: {
        title: 'Edit Task',
        entity: task,
        fields: editableFields,
        mode: 'edit'
      }
    });

    dialogRef.afterClosed()
      .pipe(
        filter(result => !!result),
        switchMap(result => this.taskService.updateTask(task.id, result)
          .pipe(
            catchError(error => {
              this.snackBar.open('Failed to update task', 'Close', {
                duration: 3000
              });
              return of(null);
            })
          )
        )
      )
      .subscribe(updatedTask => {
        if (updatedTask) {
          this.loadTasks();
          this.snackBar.open('Task updated successfully', 'Close', {
            duration: 3000
          });
        }
      });
  }

  public createTask(): void {
    const editableFields: DialogField[] = [
      { name: 'name', type: 'text', label: 'Name', required: true },
      { name: 'description', type: 'text', label: 'Description', required: false },
      { 
        name: 'status', 
        type: 'select', 
        label: 'Status', 
        required: true,
        options: [
          { value: 'New', display: 'New' },
          { value: 'In Progress', display: 'In Progress' },
          { value: 'On Hold', display: 'On Hold' },
          { value: 'Completed', display: 'Completed' },
          { value: 'Cancelled', display: 'Cancelled' }
        ]
      },
      { 
        name: 'priority', 
        type: 'select', 
        label: 'Priority', 
        required: true,
        options: [
          { value: 'Low', display: 'Low' },
          { value: 'Medium', display: 'Medium' },
          { value: 'High', display: 'High' },
          { value: 'Critical', display: 'Critical' }
        ]
      },
      { name: 'startDate', type: 'date', label: 'Start Date', required: false },
      { name: 'dueDate', type: 'date', label: 'Due Date', required: false },
      { name: 'completionPercentage', type: 'text', label: 'Completion %', required: false },
      { name: 'estimatedHours', type: 'text', label: 'Estimated Hours', required: false },
      { name: 'actualHours', type: 'text', label: 'Actual Hours', required: false },
      { name: 'assigneeId', type: 'text', label: 'Assignee ID', required: false },
      { name: 'assigneeName', type: 'text', label: 'Assignee Name', required: false },
      { name: 'projectId', type: 'text', label: 'Project ID', required: false },
      { name: 'parentTaskId', type: 'text', label: 'Parent Task ID', required: false }
    ];

    const dialogRef = this.dialog.open(BaseDialogComponent, {
      width: Constants.DEFAULT_DIALOG_WIDTH,
      data: {
        title: 'Create Task',
        entity: {},
        fields: editableFields,
        mode: 'create'
      }
    });

    dialogRef.afterClosed()
      .pipe(
        filter(result => !!result),
        switchMap(result => this.taskService.createTask(result)
          .pipe(
            catchError(error => {
              this.snackBar.open('Failed to create task', 'Close', {
                duration: 3000
              });
              return of(null);
            })
          )
        )
      )
      .subscribe(newTask => {
        if (newTask) {
          this.loadTasks();
          this.snackBar.open('Task created successfully', 'Close', {
            duration: 3000
          });
        }
      });
  }

  public deleteTask(taskId: number, event?: Event): void {
    if (event) {
      event.stopPropagation();
    }
    
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: Constants.DIALOG_WIDTH,
      data: {
        title: 'Confirm Delete',
        message: 'Are you sure you want to delete this task?',
        confirmButton: 'Delete',
        cancelButton: 'Cancel'
      }
    });

    dialogRef.afterClosed()
      .pipe(
        filter((result: boolean) => result),
        switchMap(() => this.taskService.deleteTask(taskId)),
        catchError(error => {
          NotificationUtils.showError(this.dialog, 'Failed to delete task', error);
          return of(null);
        })
      )
      .subscribe(() => {
        this.loadTasks();
        NotificationUtils.showSuccess(this.dialog, 'Task has been deleted successfully');
      });
  }

  private setupSearchAndSort(): void {
    this.setupSearchTermSubscription();

    merge(
      this.sort.sortChange,
      this.paginator.page
    )
      .pipe(
        startWith({}),
        switchMap(() => {
          this.isLoading = true;
          const params = this.buildQueryParams();
          return this.getTasksObservable(params);
        })
      )
      .subscribe(response => this.handleTaskResponse(response));
  }

  private setupSearchTermSubscription(): void {
    this.searchTerm.pipe(
      debounceTime(300),
      distinctUntilChanged()
    ).subscribe(term => {
      this.filters.searchTerm = term;
      
      if (this.paginator) {
        this.paginator.pageIndex = 0;
      }
      
      this.loadTasks();
    });
  }

  private buildQueryParams(): QueryParams {
    return {
      page: this.paginator.pageIndex + 1,
      pageSize: this.paginator.pageSize || this.defaultPageSize,
      sortColumn: this.sort.active || 'id',
      sortDirection: this.sort.direction as 'asc' | 'desc' || 'asc',
      ...this.filters
    };
  }

  private getDefaultQueryParams(): QueryParams {
    return {
      page: 1,
      pageSize: this.defaultPageSize,
      sortColumn: 'id',
      sortDirection: 'asc'
    };
  }

  private getTasksObservable(params: QueryParams): Observable<TaskResponseDto> {
    return (params.searchTerm && params.searchTerm.length >= 3)
      ? this.taskService.searchTasks(params).pipe(
          catchError(error => this.handleError(error, 'search'))
        )
      : this.taskService.getTasks(params).pipe(
          catchError(error => this.handleError(error, 'fetch'))
        );
  }

  private handleError(error: any, operation: string): Observable<TaskResponseDto> {
    console.error(`Error ${operation}ing tasks:`, error);
    NotificationUtils.showError(this.dialog, `Failed to ${operation} tasks`, error);
    return of({items: [], total: 0, page: 1, pageSize: this.defaultPageSize});
  }

  private fetchData(params: QueryParams): void {
    this.isLoading = true;
    this.getTasksObservable(params).subscribe(response => this.handleTaskResponse(response));
  }

  private handleTaskResponse(response: TaskResponseDto): void {
    this.dataSource.data = response.items;
    this.totalCount = response.total;
    this.isLoading = false;
    
    if (this.paginator) {
      this.paginator.pageIndex = response.page - 1;
    }
    
    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }
}
