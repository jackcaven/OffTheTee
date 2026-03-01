import { useEffect, useState } from 'react';
import { useForm, useFieldArray, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { useMutation } from '@tanstack/react-query';
import { useNavigate, Link } from 'react-router-dom';
import { courseSchema, type CourseFormValues } from '../lib/schemas/courses';
import { ExternalCourseSearch } from '../features/courses/ExternalCourseSearch';
import apiClient from '../lib/api';
import type { CourseDto } from '../types/courses';

const HOLE_COUNTS = [9, 12, 18] as const;

function buildDefaultHoles(count: number) {
  return Array.from({ length: count }, (_, i) => ({
    holeNumber: i + 1,
    par: 4 as number,
    strokeIndex: i + 1,
    yardage: undefined as number | undefined,
  }));
}

export function CourseCreatePage() {
  const navigate = useNavigate();
  const [showExternalSearch, setShowExternalSearch] = useState(false);

  const {
    register,
    handleSubmit,
    control,
    watch,
    formState: { errors },
  } = useForm<CourseFormValues>({
    resolver: zodResolver(courseSchema),
    defaultValues: {
      name: '',
      location: '',
      holeCount: 18,
      slopeRating: undefined,
      courseRating: undefined,
      holes: buildDefaultHoles(18),
    },
  });

  const { fields, replace } = useFieldArray({ control, name: 'holes' });

  const holeCount = watch('holeCount');

  // Regenerate holes when holeCount changes
  useEffect(() => {
    replace(buildDefaultHoles(holeCount));
  }, [holeCount, replace]);

  const mutation = useMutation({
    mutationFn: async (data: CourseFormValues) => {
      const response = await apiClient.post<CourseDto>('/courses', data);
      return response.data;
    },
    onSuccess: (course) => {
      navigate(`/courses/${course.id}`);
    },
  });

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-4xl mx-auto px-4 py-8">
        <nav className="text-sm text-gray-500 mb-4">
          <Link to="/courses" className="hover:text-green-600">
            Courses
          </Link>{' '}
          / New course
        </nav>

        <h1 className="text-2xl font-bold text-gray-900 mb-6">Add course</h1>

        {/* External search panel */}
        <div className="mb-6">
          {!showExternalSearch ? (
            <button
              type="button"
              onClick={() => setShowExternalSearch(true)}
              className="text-sm text-green-600 hover:text-green-700 font-medium"
            >
              Search course database instead →
            </button>
          ) : (
            <ExternalCourseSearch onClose={() => setShowExternalSearch(false)} />
          )}
        </div>

        <form
          onSubmit={handleSubmit((data) => mutation.mutate(data))}
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
                  <span className="text-gray-400 font-normal">(optional, 55–155)</span>
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
                  <span className="text-gray-400 font-normal">(optional, 60–80)</span>
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

            {errors.holes?.root?.message && (
              <p className="mb-3 text-sm text-red-600">{errors.holes.root.message}</p>
            )}
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
                        {errors.holes?.[index]?.par && (
                          <p className="mt-0.5 text-xs text-red-600">
                            {errors.holes[index]?.par?.message}
                          </p>
                        )}
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
                        {errors.holes?.[index]?.strokeIndex && (
                          <p className="mt-0.5 text-xs text-red-600">
                            {errors.holes[index]?.strokeIndex?.message}
                          </p>
                        )}
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

          {mutation.isError && (
            <p className="text-sm text-red-600">{(mutation.error as Error).message}</p>
          )}

          <div className="flex items-center gap-4">
            <button
              type="submit"
              disabled={mutation.isPending}
              className="px-6 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500 disabled:opacity-50 disabled:cursor-not-allowed"
            >
              {mutation.isPending ? 'Saving…' : 'Save course'}
            </button>
            <Link to="/courses" className="text-sm text-gray-600 hover:text-gray-800">
              Cancel
            </Link>
          </div>
        </form>
      </div>
    </div>
  );
}
