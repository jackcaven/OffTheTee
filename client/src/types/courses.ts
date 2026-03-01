export interface CourseHoleDto {
  holeNumber: number;
  par: number;
  strokeIndex: number;
  yardage?: number;
}

export interface CourseDto {
  id: string;
  name: string;
  location: string;
  holeCount: number;
  slopeRating?: number;
  courseRating?: number;
  courseDataSource: string;
  externalCourseId?: string;
  holes: CourseHoleDto[];
}

export interface ExternalCourseDto {
  externalId: string;
  name: string;
  location: string;
  holeCount: number;
  slopeRating?: number;
  courseRating?: number;
  holes: CourseHoleDto[];
}

export interface CursorPage<T> {
  items: T[];
  nextCursor: string | null;
}
