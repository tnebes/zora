import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Task } from '../models/task';
import { TaskService } from '../../core/services/task.service';

interface EntityOption {
  value: number;
  display: string;
}

@Component({
  selector: 'app-task-edit',
  templateUrl: './task-edit.component.html',
  styleUrls: ['./task-edit.component.css']
})
export class TaskEditComponent implements OnInit {
  taskForm: FormGroup;
  taskId: number | null = null;
  isEdit: boolean = false;
  loading: boolean = false;
  error: string = '';
  statuses: {value: string, display: string}[] = [
    { value: 'NotStarted', display: 'Not Started' },
    { value: 'InProgress', display: 'In Progress' },
    { value: 'OnHold', display: 'On Hold' },
    { value: 'Completed', display: 'Completed' }
  ];
  projects: EntityOption[] = [];
  users: EntityOption[] = [];

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private route: ActivatedRoute,
    private router: Router,
    private dialog: MatDialog
  ) {
    this.taskForm = this.createForm();
  }

  ngOnInit(): void {
    // In a real application, fetch programs, projects and users from API
    this.loadRelatedEntities();

    this.route.paramMap.subscribe(params => {
      const idParam = params.get('id');
      if (idParam) {
        this.taskId = +idParam;
        this.isEdit = true;
        this.loadTask();
      }
    });
  }

  createForm(): FormGroup {
    return this.fb.group({
      name: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', Validators.required],
      status: ['NotStarted', Validators.required],
      priority: ['Medium', Validators.required],
      startDate: [null],
      dueDate: [null],
      completionPercentage: [0, [Validators.min(0), Validators.max(100)]],
      estimatedHours: [null, Validators.min(0)],
      actualHours: [null, Validators.min(0)],
      assigneeId: [null],
      projectId: [null],
      parentTaskId: [null]
    });
  }

  loadRelatedEntities(): void {
    // Mock data - in a real application, you would load these from API
    this.projects = [
      { value: 1, display: 'Project X' },
      { value: 2, display: 'Project Y' },
      { value: 11, display: 'Project Z' }
    ];
    
    this.users = [
      { value: 1, display: 'User 1' },
      { value: 2, display: 'User 2' },
      { value: 997, display: 'User 997' }
    ];
  }

  loadTask(): void {
    if (!this.taskId) return;
    
    this.loading = true;
    this.taskService.getTask(this.taskId).subscribe({
      next: (task) => {
        this.taskForm.patchValue({
          name: task.name,
          description: task.description,
          status: task.status,
          priority: task.priority,
          startDate: task.startDate ? new Date(task.startDate) : null,
          dueDate: task.dueDate ? new Date(task.dueDate) : null,
          completionPercentage: task.completionPercentage,
          estimatedHours: task.estimatedHours,
          actualHours: task.actualHours,
          assigneeId: task.assigneeId,
          projectId: task.projectId,
          parentTaskId: task.parentTaskId
        });
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Error loading task. Please try again later.';
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.taskForm.invalid) return;
    
    const taskData = this.taskForm.value;
    
    this.loading = true;
    
    if (this.isEdit && this.taskId) {
      this.taskService.updateTask(this.taskId, taskData).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (err) => {
          this.error = 'Error updating task. Please try again later.';
          this.loading = false;
        }
      });
    } else {
      this.taskService.createTask(taskData).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (err) => {
          this.error = 'Error creating task. Please try again later.';
          this.loading = false;
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/tasks']);
  }
}
