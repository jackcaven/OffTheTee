import { useState, useEffect } from 'react';
import { useParams, Link } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useForm, useFieldArray, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { courseSchema, type CourseFormValues } from '../lib/schemas/courses';
import apiClient from '../lib/api';
import type { CourseDto } from '../types/courses';

const HOLE_COUNTS = [9, 12, 18] as const;

export function CourseDetailPage() {
  const { id } = useParams<{ id: string }>();
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);

  const { data: course, isLoading, isError, error } = useQuery({
    queryKey: ['courses', id],
    queryFn: async () => {
      const response = await apiClient.get<CourseDto>(`/courses/${id}`);
      return response.data;
    },
    enabled: !!id,
  });

  const {
    register,
    handleSubmit,
    control,
    watch,
    reset,
    formState: { errors },
  } = useForm<CourseFormValues>({
    resolver: zodResolver(courseSchema),
  });

  const { fields, replace } = useFieldArray({ control, name: 'holes' });
  const holeCount = watch('holeCount');

  // Populate form when course data loads
  useEffect(() => {
    if (course) {
      reset({
        name: course.name,
        location: course.location,
        holeCount: course.holeCount as 9 | 12 | 18,
        slopeRating: course.slopeRating ?? undefined,
        courseRating: course.courseRating ?? undefined,
        holes: course.holes.map((h) => ({
          holeNumber: h.holeNumber,
          par: h.par,
          strokeIndex: h.strokeIndex,
          yardage: h.yardage ?? undefined,
        })),
      });
    }
  }, [course, reset]);

  // When holeCount changes in edit mode, resize the holes array
  useEffect(() => {
    if (!isEditing || !course) return;
    if (holeCount === course.holeCount) return;
    replace(
      Array.from({ length: holeCount }, (_, i) => ({
        holeNumber: i + 1,
        par: 4 as number,
        strokeIndex: i + 1,
        yardage: undefined as number | undefined,
      }))
    );
  }, [holeCount, isEditing, course, replace]);

  const updateMutation = useMutation({
    mutationFn: async (data: CourseFormValues) => {
      const response = await apiClient.put<CourseDto>(`/courses/${id}`, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['courses', id] });
      queryClient.invalidateQueries({ queryKey: ['courses'] });
      setIsEditing(false);
    },
  });

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-gray-500">Loading…</p>
      </div>
    );
  }

  if (isError || !course) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <p className="text-red-600">{(error as Error)?.message ?? 'Course not found'}</p>
      </div>
    );
  }

  // Read-only view
  if (!isEditing) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="max-w-4xl mx-auto px-4 py-8">
          <nav className="text-sm text-gray-500 mb-4">
            <Link to="/courses" className="hover:text-green-600">
              Courses
            </Link>{' '}
            / {course.name}
          </nav>

          <div className="flex items-start justify-between mb-6">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">{course.name}</h1>
              <p className="text-gray-500 mt-1">{course.location}</p>
            </div>
            <button
              onClick={() => setIsEditing(true)}
              className="px-4 py-2 border border-gray-300 rounded-md text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
            >
              Edit
            </button>
          </div>

          <div className="bg-white shadow rounded-lg px-6 py-6 mb-6">
            <dl className="grid grid-cols-2 gap-4 sm:grid-cols-4">
              <div>
                <dt className="text-xs font-medium text-gray-500 uppercase tracking-wider">Holes</dt>
                <dd className="mt-1 text-lg font-semibold text-gray-900">{course.holeCount}</dd>
              </div>
              <div>
                <dt className="text-xs font-medium text-gray-500 uppercase tracking-wider">Slope rating</dt>
                <dd className="mt-1 text-lg font-semibold text-gray-900">
                  {course.slopeRating ?? '—'}
                </dd>
              </div>
              <div>
                <dt className="text-xs font-medium text-gray-500 uppercase tracking-wider">Course rating</dt>
                <dd className="mt-1 text-lg font-semibold text-gray-900">
                  {course.courseRating ?? '—'}
                </dd>
              </div>
              <div>
                <dt className="text-xs font-medium text-gray-500 uppercase tracking-wider">Source</dt>
                <dd className="mt-1">
                  <span
                    className={`inline-flex px-2 py-0.5 rounded text-xs font-medium ${
                      course.courseDataSource === 'API'
                        ? 'bg-blue-100 text-blue-700'
                        : 'bg-gray-100 text-gray-600'
                    }`}
                  >
                    {course.courseDataSource}
                  </span>
                </dd>
              </div>
            </dl>
          </div>

          <div className="bg-white shadow rounded-lg px-6 py-6">
            <h2 className="text-base font-semibold text-gray-900 mb-4">Holes</h2>
            <div className="overflow-x-auto">
              <table className="min-w-full">
                <thead>
                  <tr className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider border-b border-gray-200">
                    <th className="pb-2 pr-6">Hole</th>
                    <th className="pb-2 pr-6">Par</th>
                    <th className="pb-2 pr-6">Stroke index</th>
                    <th className="pb-2">Yardage</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {course.holes.map((hole) => (
                    <tr key={hole.holeNumber}>
                      <td className="py-2 pr-6 text-sm font-medium text-gray-900">{hole.holeNumber}</td>
                      <td className="py-2 pr-6 text-sm text-gray-700">{hole.par}</td>
                      <td className="py-2 pr-6 text-sm text-gray-700">{hole.strokeIndex}</td>
                      <td className="py-2 text-sm text-gray-700">{hole.yardage ?? '—'}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      </div>
    );
  }

  // Edit mode
  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-4xl mx-auto px-4 py-8">
        <nav className="text-sm text-gray-500 mb-4">
          <Link to="/courses" className="hover:text-green-600">
            Courses
          </Link>{' '}
          / {course.name} / Edit
        </nav>

        <h1 className="text-2xl font-bold text-gray-900 mb-6">Edit course</h1>

        <form
          onSubmit={handleSubmit((data) => updateMutation.mutate(data))}
          className="space-y-6"
        >
          {/* Course details */}
          <div className="bg-white shadow rounded-lg px-6 py-6 space-y-4">
            <h2 className="text-base font-semibold text-gray-900">Course details</h2>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
              <div>
                <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                  Course name
                </label>
                <input
                  id="name"
                  type="text"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
                  {...register('name')}
                />
                {errors.name && (
                  <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
                )}
              </div>

              <div>
                <label htmlFor="location" className="block text-sm font-medium text-gray-700">
                  Location
                </label>
                <input
                  id="location"
                  type="text"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
                  {...register('location')}
                />
                {errors.location && (
                  <p className="mt-1 text-sm text-red-600">{errors.location.message}</p>
                )}
              </div>
            </div>

            <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
              <div>
                <label htmlFor="holeCount" className="block text-sm font-medium text-gray-700">
                  Number of holes
                </label>
                <Controller
                  control={control}
                  name="holeCount"
                  render={({ field }) => (
                    <select
                      id="holeCount"
                      className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
                      value={field.value}
                      onChange={(e) => field.onChange(Number(e.target.value) as 9 | 12 | 18)}
                    >
                      {HOLE_COUNTS.map((n) => (
                        <option key={n} value={n}>
                          {n}
                        </option>
                      ))}
                    </select>
                  )}
                />
              </div>

              <div>
                <label htmlFor="slopeRating" className="block text-sm font-medium text-gray-700">
                  Slope rating{' '}
                  <span className="text-gray-400 font-normal">(optional)</span>
                </label>
                <input
                  id="slopeRating"
                  type="number"
                  step="1"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
                  {...register('slopeRating', {
                    setValueAs: (v) => (v === '' ? undefined : Number(v)),
                  })}
                />
                {errors.slopeRating && (
                  <p className="mt-1 text-sm text-red-600">{errors.slopeRating.message}</p>
                )}
              </div>

              <div>
                <label htmlFor="courseRating" className="block text-sm font-medium text-gray-700">
                  Course rating{' '}
                  <span className="text-gray-400 font-normal">(optional)</span>
                </label>
                <input
                  id="courseRating"
                  type="number"
                  step="0.1"
                  className="mt-1 block w-full rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
                  {...register('courseRating', {
                    setValueAs: (v) => (v === '' ? undefined : Number(v)),
                  })}
                />
                {errors.courseRating && (
                  <p className="mt-1 text-sm text-red-600">{errors.courseRating.message}</p>
                )}
              </div>
            </div>
          </div>

          {/* Hole data */}
          <div className="bg-white shadow rounded-lg px-6 py-6">
            <h2 className="text-base font-semibold text-gray-900 mb-4">Hole data</h2>

            {typeof errors.holes?.message === 'string' && (
              <p className="mb-3 text-sm text-red-600">{errors.holes.message}</p>
            )}

            <div className="overflow-x-auto">
              <table className="min-w-full">
                <thead>
                  <tr className="text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    <th className="pb-2 pr-4 w-16">Hole</th>
                    <th className="pb-2 pr-4 w-28">Par</th>
                    <th className="pb-2 pr-4 w-36">Stroke index</th>
                    <th className="pb-2 pr-4 w-36">
                      Yardage{' '}
                      <span className="normal-case font-normal text-gray-400">(opt.)</span>
                    </th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-gray-100">
                  {fields.map((field, index) => (
                    <tr key={field.id}>
                      <td className="py-2 pr-4 text-sm font-medium text-gray-700">
                        {index + 1}
                        <input type="hidden" {...register(`holes.${index}.holeNumber`)} />
                      </td>
                      <td className="py-2 pr-4">
                        <Controller
                          control={control}
                          name={`holes.${index}.par`}
                          render={({ field: f }) => (
                            <select
                              className="w-full rounded-md border border-gray-300 px-2 py-1 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
                              value={f.value}
                              onChange={(e) => f.onChange(Number(e.target.value))}
                            >
                              <option value={3}>3</option>
                              <option value={4}>4</option>
                              <option value={5}>5</option>
                            </select>
                          )}
                        />
                      </td>
                      <td className="py-2 pr-4">
                        <input
                          type="number"
                          min={1}
                          max={holeCount}
                          className="w-full rounded-md border border-gray-300 px-2 py-1 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
                          {...register(`holes.${index}.strokeIndex`, {
                            valueAsNumber: true,
                          })}
                        />
                      </td>
                      <td className="py-2 pr-4">
                        <input
                          type="number"
                          min={1}
                          className="w-full rounded-md border border-gray-300 px-2 py-1 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
                          {...register(`holes.${index}.yardage`, {
                            setValueAs: (v) => (v === '' ? undefined : Number(v)),
                          })}
                        />
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {updateMutation.isError && (
            <p className="text-sm text-red-600">{(updateMutation.error as Error).message}</p>
          )}

          <div className="flex items-center gap-4">
            <button
              type="submit"
              disabled={updateMutation.isPending}
              className="px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {updateMutation.isPending ? 'Saving…' : 'Save changes'}
            </button>
            <button
              type="button"
              onClick={() => {
                setIsEditing(false);
                if (course) {
                  reset({
                    name: course.name,
                    location: course.location,
                    holeCount: course.holeCount as 9 | 12 | 18,
                    slopeRating: course.slopeRating ?? undefined,
                    courseRating: course.courseRating ?? undefined,
                    holes: course.holes.map((h) => ({
                      holeNumber: h.holeNumber,
                      par: h.par,
                      strokeIndex: h.strokeIndex,
                      yardage: h.yardage ?? undefined,
                    })),
                  });
                }
              }}
              className="text-sm text-gray-600 hover:text-gray-800"
            >
              Cancel
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
