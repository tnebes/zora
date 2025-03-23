export interface Task {
    id: number;
    name: string;
    description: string;
    status: string;
    startDate?: string;
    dueDate?: string;
    completionPercentage?: number;
    estimatedHours?: number;
    actualHours?: number;
    createdAt: string;
    createdById: number;
    updatedAt?: string | null;
    updatedById?: number | null;
    assigneeId?: number;
    assigneeName?: string;
    projectId?: number;
    priority?: string;
    parentTaskId?: number | null;
}

export interface TaskResponseDto {
    items: Task[];
    total: number;
    page: number;
    pageSize: number;
}
