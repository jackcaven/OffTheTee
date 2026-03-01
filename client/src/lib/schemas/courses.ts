import { z } from 'zod';

export const courseHoleSchema = z.object({
  holeNumber: z.number().int().min(1),
  par: z.number().int().min(3).max(5),
  strokeIndex: z.number().int().min(1),
  yardage: z.number().int().positive().optional().or(z.literal(undefined)),
});

export const courseSchema = z
  .object({
    name: z.string().min(1, 'Name is required').max(200, 'Name must not exceed 200 characters'),
    location: z
      .string()
      .min(1, 'Location is required')
      .max(300, 'Location must not exceed 300 characters'),
    holeCount: z.union([z.literal(9), z.literal(12), z.literal(18)], {
      message: 'Hole count must be 9, 12, or 18',
    }),
    slopeRating: z
      .number()
      .min(55, 'Slope rating must be at least 55')
      .max(155, 'Slope rating must not exceed 155')
      .optional()
      .or(z.literal(undefined)),
    courseRating: z
      .number()
      .min(60, 'Course rating must be at least 60')
      .max(80, 'Course rating must not exceed 80')
      .optional()
      .or(z.literal(undefined)),
    holes: z.array(courseHoleSchema),
  })
  .refine((data) => data.holes.length === data.holeCount, {
    message: 'Number of holes must match the selected hole count',
    path: ['holes'],
  })
  .refine(
    (data) => {
      const siValues = data.holes.map((h) => h.strokeIndex);
      return new Set(siValues).size === siValues.length;
    },
    {
      message: 'Stroke index values must be unique',
      path: ['holes'],
    }
  );

export type CourseFormValues = z.infer<typeof courseSchema>;
