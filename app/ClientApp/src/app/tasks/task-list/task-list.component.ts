import { AfterViewInit, Component, OnInit, ViewChild } from '@angular/core';
import { MatTableDataSource } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { MatPaginator } from '@angular/material/paginator';
import { MatSort } from '@angular/material/sort';
import { Subject, catchError, of, merge } from 'rxjs';
import { filter, startWith, switchMap } from 'rxjs/operators';
import { Router } from '@angular/router';
import { Task, TaskResponseDto } from '../models/task';
import { TaskService } from '../../core/services/task.service';
import { QueryService } from '../../core/services/query.service';
import { Constants } from 'src/app/core/constants';
import { QueryParams } from '../../core/models/query-params.interface';
import { NotificationUtils } from '../../core/utils/notification.utils';
import { ConfirmDialogComponent } from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-task-list',
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.css']
})
export class TaskListComponent implements OnInit, AfterViewInit {
  public readonly displayedColumns: string[] = ['id', 'title', 'status', 'dueDate', 'assignedUserName', 'actions'];
  public readonly dataSource: MatTableDataSource<Task> = new MatTableDataSource<Task>([]);
  public searchTerm: Subject<string> = new Subject<string>();
  public isLoading: boolean = false;
  public totalCount: number = 0;
  public currentSearchValue: string = '';
  public errorMessage: string = '';
  public readonly defaultPageSize: number = Constants.DEFAULT_PAGE_SIZE;

  @ViewChild(MatPaginator) public paginator!: MatPaginator;
  @ViewChild(MatSort) private readonly sort!: MatSort;

  constructor(
    private readonly taskService: TaskService,
    private readonly router: Router,
    private readonly dialog: MatDialog,
    private readonly queryService: QueryService
  ) { }

  ngOnInit(): void {
    this.isLoading = true;
    this.taskService.getTasks({
      page: 1,
      pageSize: this.defaultPageSize,
      sortColumn: 'id',
      sortDirection: 'asc'
    })
      .pipe(
        catchError(error => {
          console.error('Error fetching tasks:', error);
          NotificationUtils.showError(this.dialog, 'Failed to fetch tasks', error);
          return of({items: [], totalCount: 0, pageSize: this.defaultPageSize, currentPage: 1});
        })
      )
      .subscribe((response: TaskResponseDto) => {
        this.dataSource.data = response.items;
        this.totalCount = response.totalCount;
        this.isLoading = false;
      });
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
      this.setupSearchAndSort();
    }
  }

  public onSearch(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.currentSearchValue = target.value;
    this.searchTerm.next(this.currentSearchValue);
  }

  public viewTask(taskId: number): void {
    this.router.navigate(['/tasks/detail', taskId]);
  }

  public editTask(taskId: number): void {
    this.router.navigate(['/tasks/edit', taskId]);
  }

  public createTask(): void {
    this.router.navigate(['/tasks/create']);
  }

  public deleteTask(taskId: number): void {
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
    merge(
      this.sort.sortChange,
      this.paginator.page
    )
      .pipe(
        startWith({}),
        switchMap(() => {
          this.isLoading = true;
          const params: QueryParams = {
            page: this.paginator.pageIndex + 1,
            pageSize: this.paginator.pageSize || this.defaultPageSize,
            sortColumn: this.sort.active,
            sortDirection: this.sort.direction as 'asc' | 'desc'
          };

          return this.taskService.getTasks(params)
            .pipe(
              catchError(error => {
                console.error('Error fetching tasks:', error);
                NotificationUtils.showError(this.dialog, 'Failed to fetch tasks', error);
                return of({items: [], totalCount: 0, pageSize: this.defaultPageSize, currentPage: 1});
              })
            );
        })
      )
      .subscribe((response: TaskResponseDto) => {
        this.dataSource.data = response.items;
        this.totalCount = response.totalCount;
        this.isLoading = false;
        
        if (this.sort) {
          this.dataSource.sort = this.sort;
        }
      });
  }
}
