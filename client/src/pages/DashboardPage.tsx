import { Link } from 'react-router-dom';
import { useAuthStore } from '../store/authStore';

export function DashboardPage() {
  const { user, clearAuth } = useAuthStore();

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Nav */}
      <nav className="bg-white shadow-sm border-b border-gray-200">
        <div className="max-w-5xl mx-auto px-4 py-3 flex items-center justify-between">
          <span className="text-lg font-bold text-gray-900">Off The Tee</span>
          <div className="flex items-center gap-4">
            <span className="text-sm text-gray-500">{user?.displayName}</span>
            <button
              onClick={clearAuth}
              className="text-sm text-gray-500 hover:text-gray-700"
            >
              Sign out
            </button>
          </div>
        </div>
      </nav>

      {/* Content */}
      <div className="max-w-5xl mx-auto px-4 py-10">
        <h1 className="text-2xl font-bold text-gray-900 mb-2">Dashboard</h1>
        <p className="text-gray-500 mb-8">Welcome back, {user?.displayName}.</p>

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <Link
            to="/courses"
            className="bg-white rounded-lg shadow border border-gray-200 p-6 hover:border-green-400 hover:shadow-md transition-all group"
          >
            <div className="text-3xl mb-3">⛳</div>
            <h2 className="text-base font-semibold text-gray-900 group-hover:text-green-700">
              Courses
            </h2>
            <p className="mt-1 text-sm text-gray-500">
              Add and manage golf courses, hole data, slope and course ratings.
            </p>
          </Link>

          <div className="bg-white rounded-lg shadow border border-gray-200 p-6 opacity-50 cursor-not-allowed">
            <div className="text-3xl mb-3">🏆</div>
            <h2 className="text-base font-semibold text-gray-900">Tournaments</h2>
            <p className="mt-1 text-sm text-gray-500">Coming in Phase 4.</p>
          </div>

          <div className="bg-white rounded-lg shadow border border-gray-200 p-6 opacity-50 cursor-not-allowed">
            <div className="text-3xl mb-3">👤</div>
            <h2 className="text-base font-semibold text-gray-900">Players</h2>
            <p className="mt-1 text-sm text-gray-500">Coming in Phase 5.</p>
          </div>
        </div>
      </div>
    </div>
  );
}
