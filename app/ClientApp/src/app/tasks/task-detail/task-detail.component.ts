import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Task } from '../models/task';
import { TaskService } from '../../core/services/task.service';
import { MatDialog } from '@angular/material/dialog';
import { BaseDialogComponent, DialogField } from '../../shared/components/base-dialog/base-dialog.component';
import { ViewOnlyDialogComponent } from '../../shared/components/view-only-dialog/view-only-dialog.component';

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

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private taskService: TaskService,
    private dialog: MatDialog
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.taskId = +idParam;
        this.loadTask();
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
      },
      error: (err) => {
        this.error = 'Error loading task. Please try again later.';
        this.loading = false;
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
      { name: 'assigneeId', type: 'text', label: 'Assignee ID', required: false },
      { name: 'projectId', type: 'text', label: 'Project ID', required: false },
      { name: 'parentTaskId', type: 'text', label: 'Parent Task ID', required: false },
      { name: 'createdAt', type: 'text', label: 'Created At', required: false, isDate: true },
      { name: 'updatedAt', type: 'text', label: 'Updated At', required: false, isDate: true }
    ];
  }

  showTaskDetails(): void {
    if (!this.task) return;
    
    this.dialog.open(ViewOnlyDialogComponent, {
      width: '600px',
      data: {
        title: 'Task Details',
        entity: this.task,
        fields: this.dialogFields
      }
    });
  }

  editTask(): void {
    this.router.navigate(['/tasks/edit', this.taskId]);
  }

  deleteTask(): void {
    if (confirm('Are you sure you want to delete this task?')) {
      this.taskService.deleteTask(this.taskId).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (err) => {
          this.error = 'Error deleting task. Please try again later.';
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/tasks']);
  }
}
