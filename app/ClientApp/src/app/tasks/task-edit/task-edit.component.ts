import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Task } from '../models/task';
import { TaskService } from '../task.service';

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
    { value: 'Todo', display: 'Todo' },
    { value: 'InProgress', display: 'In Progress' },
    { value: 'Done', display: 'Done' }
  ];
  programs: EntityOption[] = [];
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
      title: ['', [Validators.required, Validators.minLength(3)]],
      description: ['', Validators.required],
      status: ['Todo', Validators.required],
      dueDate: [null],
      assignedUserId: [null],
      programId: [null],
      projectId: [null]
    });
  }

  loadRelatedEntities(): void {
    // Mock data - in a real application, you would load these from API
    this.programs = [
      { value: 1, display: 'Program A' },
      { value: 2, display: 'Program B' }
    ];
    
    this.projects = [
      { value: 1, display: 'Project X' },
      { value: 2, display: 'Project Y' }
    ];
    
    this.users = [
      { value: 1, display: 'User 1' },
      { value: 2, display: 'User 2' }
    ];
  }

  loadTask(): void {
    if (!this.taskId) return;
    
    this.loading = true;
    this.taskService.getTask(this.taskId).subscribe({
      next: (task) => {
        this.taskForm.patchValue({
          title: task.title,
          description: task.description,
          status: task.status,
          dueDate: task.dueDate ? new Date(task.dueDate) : null,
          assignedUserId: task.assignedUserId,
          programId: task.programId,
          projectId: task.projectId
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
