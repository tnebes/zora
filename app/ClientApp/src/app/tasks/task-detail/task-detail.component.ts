import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Task } from '../models/task';
import { TaskService } from '../../core/services/task.service';
import { MatDialog } from '@angular/material/dialog';
import { BaseDialogComponent, DialogField } from '../../shared/components/base-dialog/base-dialog.component';
import { ViewOnlyDialogComponent } from '../../shared/components/view-only-dialog/view-only-dialog.component';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Constants } from '../../core/constants';
import { ConfirmDialogComponent } from 'src/app/shared/components/confirm-dialog/confirm-dialog.component';
import { NotificationUtils } from '../../core/utils/notification.utils';
import { filter, catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { switchMap } from 'rxjs/operators';
import { UserService } from '../../core/services/user.service';
import { AssetService } from '../../core/services/asset.service';
import { AssetResponse } from '../../core/models/asset.interface';
import { AssetDialogComponent } from '../../components/assets/asset-dialog/asset-dialog.component';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

@Component({
  selector: 'app-task-detail',
  templateUrl: './task-detail.component.html',
  styleUrls: ['./task-detail.component.css']
})
export class TaskDetailComponent implements OnInit {
  task: Task | null = null;
  taskId: number = 0;
  loading: boolean = true;
  error: string = '';
  dialogFields: DialogField[] = [];
  editableFields: DialogField[] = [];
  assets: AssetResponse[] = [];
  loadingAssets: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private taskService: TaskService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar,
    private userService: UserService,
    private assetService: AssetService
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.taskId = +idParam;
        this.loadTask();
        this.loadAssets();
      }
    });
  }

  loadTask(): void {
    this.loading = true;
    this.taskService.getTask(this.taskId).subscribe({
      next: (task) => {
        this.task = task;
        this.loading = false;
        this.setupDialogFields();
        this.setupEditableFields();
      },
      error: (err) => {
        this.error = 'Error loading task. Please try again later.';
        this.loading = false;
      }
    });
  }

  loadAssets(): void {
    this.loadingAssets = true;
    this.assetService.getAssetsByTaskId(this.taskId).subscribe({
      next: (response) => {
        this.assets = response.items;
        this.loadingAssets = false;
      },
      error: (err) => {
        this.error = 'Error loading assets. Please try again later.';
        this.loadingAssets = false;
      }
    });
  }

  setupDialogFields(): void {
    if (!this.task) return;
    
    this.dialogFields = [
      { name: 'name', type: 'text', label: 'Name', required: true },
      { name: 'description', type: 'text', label: 'Description', required: false },
      { name: 'status', type: 'text', label: 'Status', required: true },
      { name: 'priority', type: 'text', label: 'Priority', required: true },
      { name: 'startDate', type: 'text', label: 'Start Date', required: false, isDate: true },
      { name: 'dueDate', type: 'text', label: 'Due Date', required: false, isDate: true },
      { name: 'completionPercentage', type: 'text', label: 'Completion %', required: false },
      { name: 'estimatedHours', type: 'text', label: 'Estimated Hours', required: false },
      { name: 'actualHours', type: 'text', label: 'Actual Hours', required: false },
      { name: 'assigneeName', type: 'text', label: 'Assignee', required: false },
      { name: 'createdAt', type: 'text', label: 'Created At', required: false, isDate: true },
      { name: 'updatedAt', type: 'text', label: 'Updated At', required: false, isDate: true }
    ];
  }

  setupEditableFields(): void {
    if (!this.task) return;

    this.editableFields = [
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
      { 
        name: 'assigneeId', 
        type: 'select', 
        label: 'Assignee', 
        required: false,
        options: [],
        searchable: true,
        searchService: this.userService,
        searchMethod: 'searchUsersByTerm',
        displayField: 'username',
        valueField: 'id'
      }
    ];
  }

  showTaskDetails(): void {
    if (!this.task) return;
    
    this.dialog.open(ViewOnlyDialogComponent, {
      width: Constants.VIEW_ONLY_DIALOG_WIDTH,
      data: {
        title: 'Task Details',
        entity: this.task,
        fields: this.dialogFields
      }
    });
  }

  editTask(): void {
    if (!this.task) return;

    const dialogRef = this.dialog.open(BaseDialogComponent, {
      width: Constants.DEFAULT_DIALOG_WIDTH,
      data: {
        title: 'Edit Task',
        entity: this.task,
        fields: this.editableFields,
        mode: 'edit'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.updateTask(result);
      }
    });
  }

  updateTask(updatedTask: Partial<Task>): void {
    this.loading = true;
    
    this.taskService.updateTask(this.taskId, updatedTask).subscribe({
      next: (task) => {
        this.task = task;
        this.loading = false;
        this.snackBar.open('Task updated successfully', 'Close', {
          duration: 3000
        });
      },
      error: (err) => {
        this.error = 'Error updating task. Please try again later.';
        this.loading = false;
        this.snackBar.open('Failed to update task', 'Close', {
          duration: 3000
        });
      }
    });
  }

  deleteTask(): void {
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
        switchMap(() => this.taskService.deleteTask(this.taskId)),
        catchError(error => {
          NotificationUtils.showError(this.dialog, 'Failed to delete task', error);
          return of(null);
        })
      )
      .subscribe(() => {
        NotificationUtils.showSuccess(this.dialog, 'Task has been deleted successfully');
        this.router.navigate(['/tasks']);
      });
  }

  goBack(): void {
    this.router.navigate(['/tasks']);
  }

  openAssetDialog(asset: AssetResponse): void {
    this.dialog.open(AssetDialogComponent, {
      width: Constants.DEFAULT_DIALOG_WIDTH,
      data: asset
    });
  }
}
