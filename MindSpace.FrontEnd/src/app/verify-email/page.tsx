'use client';

import { useEffect, useState, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { authService } from '@/services/auth';
import { useAuth } from '@/contexts/AuthContext';
import { Edit3, CheckCircle, XCircle, Loader } from 'lucide-react';
import Link from 'next/link';
import Button from '@/components/ui/Button';

function VerifyEmailContent() {
  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [message, setMessage] = useState('');
  const searchParams = useSearchParams();
  const router = useRouter();
  const { refreshUser } = useAuth();

  useEffect(() => {
    const token = searchParams.get('token');
    if (!token) {
      setStatus('error');
      setMessage('Link de verificare invalid.');
      return;
    }

    authService.verifyEmail(token).then(async (response) => {
      if (response.success) {
        setStatus('success');
        setMessage(response.message);
        await refreshUser();
        setTimeout(() => router.push('/'), 3000);
      } else {
        setStatus('error');
        setMessage(response.message || 'Verificarea a eșuat.');
      }
    }).catch(() => {
      setStatus('error');
      setMessage('A apărut o eroare. Vă rugăm să încercați din nou.');
    });
  }, [searchParams, router, refreshUser]);

  return (
    <div className="min-h-screen bg-gradient-to-br from-purple-50 via-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="relative w-full max-w-md text-center">
        <Link href="/" className="inline-flex items-center space-x-2 mb-8">
          <div className="w-12 h-12 bg-gradient-to-r from-purple-600 to-blue-600 rounded-xl flex items-center justify-center shadow-lg">
            <Edit3 className="h-6 w-6 text-white" />
          </div>
          <span className="text-2xl font-bold bg-gradient-to-r from-purple-600 to-blue-600 bg-clip-text text-transparent">
            MindSpace
          </span>
        </Link>

        {status === 'loading' && (
          <>
            <div className="w-20 h-20 bg-gradient-to-r from-purple-100 to-blue-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <Loader className="h-10 w-10 text-purple-600 animate-spin" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Se verifică...</h1>
            <p className="text-gray-500">Adresa dvs. de e-mail este în curs de verificare, vă rugăm să așteptați.</p>
          </>
        )}

        {status === 'success' && (
          <>
            <div className="w-20 h-20 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <CheckCircle className="h-10 w-10 text-green-600" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">E-mail verificat!</h1>
            <p className="text-gray-600 mb-6">{message}</p>
            <p className="text-gray-400 text-sm mb-4">Sunteți redirecționat către pagina principală...</p>
            <Button
              onClick={() => router.push('/')}
              className="w-full h-12 bg-gradient-to-r from-purple-600 to-blue-600 text-white rounded-xl font-semibold"
            >
              Mergeți la pagina principală
            </Button>
          </>
        )}

        {status === 'error' && (
          <>
            <div className="w-20 h-20 bg-red-100 rounded-full flex items-center justify-center mx-auto mb-6">
              <XCircle className="h-10 w-10 text-red-600" />
            </div>
            <h1 className="text-2xl font-bold text-gray-900 mb-2">Verificarea a eșuat</h1>
            <p className="text-gray-600 mb-8">{message}</p>
            <Link href="/login">
              <Button className="w-full h-12 bg-gradient-to-r from-purple-600 to-blue-600 text-white rounded-xl font-semibold">
                Înapoi la pagina de autentificare
              </Button>
            </Link>
          </>
        )}
      </div>
    </div>
  );
}

export default function VerifyEmailPage() {
  return (
    <Suspense fallback={
      <div className="min-h-screen bg-gradient-to-br from-purple-50 via-blue-50 to-indigo-100 flex items-center justify-center">
        <Loader className="h-10 w-10 text-purple-600 animate-spin" />
      </div>
    }>
      <VerifyEmailContent />
    </Suspense>
  );
}
