import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import apiClient from '../lib/api';
import type { CursorPage, CourseDto } from '../types/courses';

export function CoursesPage() {
  const [cursor, setCursor] = useState<string | null>(null);

  const { data, isLoading, isError, error } = useQuery({
    queryKey: ['courses', cursor],
    queryFn: async () => {
      const params = new URLSearchParams({ limit: '20' });
      if (cursor) params.set('cursor', cursor);
      const response = await apiClient.get<CursorPage<CourseDto>>(`/courses?${params}`);
      return response.data;
    },
  });

  return (
    <div className="min-h-screen bg-gray-50">
      <div className="max-w-5xl mx-auto px-4 py-8">
        <div className="flex items-center justify-between mb-6">
          <h1 className="text-2xl font-bold text-gray-900">Courses</h1>
          <Link
            to="/courses/new"
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500"
          >
            Add course
          </Link>
        </div>

        {isLoading && (
          <p className="text-gray-500 text-center py-12">Loading courses…</p>
        )}

        {isError && (
          <p className="text-red-600 text-center py-12">
            {(error as Error).message}
          </p>
        )}

        {data && data.items.length === 0 && (
          <div className="text-center py-16">
            <p className="text-gray-500 text-lg">No courses yet.</p>
            <p className="text-gray-400 mt-1">
              Add one manually or import from the course database.
            </p>
          </div>
        )}

        {data && data.items.length > 0 && (
          <>
            <div className="bg-white shadow rounded-lg overflow-hidden">
              <table className="min-w-full divide-y divide-gray-200">
                <thead className="bg-gray-50">
                  <tr>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Name
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Location
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Holes
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Slope / Course Rating
                    </th>
                    <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                      Source
                    </th>
                  </tr>
                </thead>
                <tbody className="bg-white divide-y divide-gray-200">
                  {data.items.map((course) => (
                    <tr key={course.id} className="hover:bg-gray-50">
                      <td className="px-6 py-4 whitespace-nowrap">
                        <Link
                          to={`/courses/${course.id}`}
                          className="text-green-600 hover:text-green-700 font-medium"
                        >
                          {course.name}
                        </Link>
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                        {course.location}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                        {course.holeCount}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-600">
                        {course.slopeRating != null && course.courseRating != null
                          ? `${course.slopeRating} / ${course.courseRating}`
                          : '—'}
                      </td>
                      <td className="px-6 py-4 whitespace-nowrap text-sm">
                        <span
                          className={`inline-flex px-2 py-0.5 rounded text-xs font-medium ${
                            course.courseDataSource === 'API'
                              ? 'bg-blue-100 text-blue-700'
                              : 'bg-gray-100 text-gray-600'
                          }`}
                        >
                          {course.courseDataSource}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {data.nextCursor && (
              <div className="mt-4 text-center">
                <button
                  onClick={() => setCursor(data.nextCursor)}
                  className="px-4 py-2 text-sm font-medium text-green-600 hover:text-green-700"
                >
                  Load more
                </button>
              </div>
            )}
          </>
        )}
      </div>
    </div>
  );
}
