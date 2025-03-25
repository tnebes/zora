export interface CreateTaskDto {
  name: string;
  description?: string;
  status: string;
  startDate?: Date;
  dueDate?: Date;
  completionPercentage?: number;
  estimatedHours?: number;
  actualHours?: number;
  assigneeId?: number;
  projectId?: number;
  priority?: string;
  parentTaskId?: number;
} 