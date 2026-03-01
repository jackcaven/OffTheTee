import { useState } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import apiClient from '../../lib/api';
import type { ExternalCourseDto, CourseDto } from '../../types/courses';

interface ExternalCourseSearchProps {
  onClose: () => void;
}

export function ExternalCourseSearch({ onClose }: ExternalCourseSearchProps) {
  const navigate = useNavigate();
  const [searchName, setSearchName] = useState('');
  const [submitted, setSubmitted] = useState('');

  const { data: results, isFetching, isError } = useQuery({
    queryKey: ['external-courses', submitted],
    queryFn: async () => {
      const response = await apiClient.get<ExternalCourseDto[]>(
        `/courses/search?name=${encodeURIComponent(submitted)}`
      );
      return response.data;
    },
    enabled: submitted.length > 0,
  });

  const importMutation = useMutation({
    mutationFn: async (externalId: string) => {
      const response = await apiClient.post<CourseDto>('/courses/import', { externalId });
      return response.data;
    },
    onSuccess: (course) => {
      navigate(`/courses/${course.id}`);
    },
  });

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchName.trim()) setSubmitted(searchName.trim());
  };

  return (
    <div className="border border-gray-200 rounded-lg p-4 bg-gray-50 space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-sm font-semibold text-gray-700">Search course database</h3>
        <button
          type="button"
          onClick={onClose}
          className="text-gray-400 hover:text-gray-600 text-sm"
        >
          ✕ Close
        </button>
      </div>

      <form onSubmit={handleSearch} className="flex gap-2">
        <input
          type="text"
          value={searchName}
          onChange={(e) => setSearchName(e.target.value)}
          placeholder="e.g. Sunningdale"
          className="flex-1 rounded-md border border-gray-300 px-3 py-2 text-sm focus:border-green-500 focus:outline-none focus:ring-1 focus:ring-green-500"
        />
        <button
          type="submit"
          disabled={isFetching || !searchName.trim()}
          className="px-4 py-2 text-sm font-medium text-white bg-green-600 hover:bg-green-700 rounded-md disabled:opacity-50 disabled:cursor-not-allowed"
        >
          {isFetching ? 'Searching…' : 'Search'}
        </button>
      </form>

      {isError && (
        <p className="text-sm text-red-600">Search unavailable — please fill in details manually.</p>
      )}

      {results && results.length === 0 && submitted && (
        <p className="text-sm text-gray-500">
          No results found for "{submitted}" — fill in details manually below.
        </p>
      )}

      {results && results.length > 0 && (
        <ul className="divide-y divide-gray-200 bg-white rounded-md border border-gray-200 max-h-64 overflow-y-auto">
          {results.map((course) => (
            <li key={course.externalId} className="flex items-center justify-between px-4 py-3">
              <div>
                <p className="text-sm font-medium text-gray-900">{course.name}</p>
                <p className="text-xs text-gray-500">
                  {course.location} · {course.holeCount} holes
                  {course.slopeRating != null ? ` · Slope ${course.slopeRating}` : ''}
                </p>
              </div>
              <button
                type="button"
                disabled={importMutation.isPending}
                onClick={() => importMutation.mutate(course.externalId)}
                className="ml-4 px-3 py-1 text-xs font-medium text-white bg-green-600 hover:bg-green-700 rounded disabled:opacity-50 disabled:cursor-not-allowed"
              >
                {importMutation.isPending ? 'Importing…' : 'Import'}
              </button>
            </li>
          ))}
        </ul>
      )}

      {importMutation.isError && (
        <p className="text-sm text-red-600">{(importMutation.error as Error).message}</p>
      )}
    </div>
  );
}
