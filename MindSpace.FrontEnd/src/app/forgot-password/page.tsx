'use client';

import { useState } from 'react';
import { authService } from '@/services/auth';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { Mail, ArrowRight, ArrowLeft, Edit3, CheckCircle } from 'lucide-react';
import Link from 'next/link';

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState('');
  const [submitted, setSubmitted] = useState(false);
  const [resetToken, setResetToken] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    setError('');

    try {
      const result = await authService.forgotPassword(email);
      if (result.success) {
        setSubmitted(true);
        // În modul de dezvoltare se returnează tokenul, să îl arătăm utilizatorului
        if (result.resetToken) {
          setResetToken(result.resetToken);
        }
      } else {
        setError('A apărut o eroare. Vă rugăm să încercați din nou.');
      }
    } catch {
      setError('A apărut o eroare. Vă rugăm să încercați din nou.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-purple-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="absolute inset-0 overflow-hidden">
        <div className="absolute -top-40 -right-40 w-80 h-80 bg-gradient-to-br from-blue-400/20 to-purple-400/20 rounded-full blur-3xl"></div>
        <div className="absolute -bottom-40 -left-40 w-80 h-80 bg-gradient-to-tr from-purple-400/20 to-pink-400/20 rounded-full blur-3xl"></div>
      </div>

      <div className="relative w-full max-w-md">
        <div className="text-center mb-8">
          <Link href="/" className="inline-flex items-center space-x-2 mb-6">
            <div className="w-12 h-12 bg-gradient-to-r from-blue-600 to-purple-600 rounded-xl flex items-center justify-center shadow-lg">
              <Edit3 className="h-6 w-6 text-white" />
            </div>
            <span className="text-2xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent">
              MindSpace
            </span>
          </Link>
        </div>

        <Card className="backdrop-blur-xl bg-white/80 shadow-2xl border-0 rounded-2xl">
          <CardHeader className="space-y-1 pb-6">
            <CardTitle className="text-2xl font-bold text-center text-gray-900">
              {submitted ? 'E-mail trimis' : 'Am uitat parola'}
            </CardTitle>
          </CardHeader>
          <CardContent>
            {submitted ? (
              <div className="text-center space-y-6">
                <div className="flex justify-center">
                  <CheckCircle className="h-16 w-16 text-green-500" />
                </div>
                <p className="text-gray-600">
                  A fost trimis un link de resetare a parolei la <strong>{email}</strong>.
                  Verificați-vă inboxul.
                </p>

                {resetToken && (
                  <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-4 text-left">
                    <p className="text-xs font-semibold text-yellow-800 mb-2">
                      🛠️ Mod dezvoltator — Token de resetare:
                    </p>
                    <p className="text-xs text-yellow-700 break-all font-mono">{resetToken}</p>
                    <Link
                      href={`/reset-password?email=${encodeURIComponent(email)}&token=${encodeURIComponent(resetToken)}`}
                      className="inline-block mt-3 text-xs text-blue-600 hover:text-blue-700 font-medium underline"
                    >
                      Resetați parola →
                    </Link>
                  </div>
                )}

                <Link href="/login">
                  <Button className="w-full h-12 bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white font-semibold rounded-xl">
                    Înapoi la pagina de autentificare
                  </Button>
                </Link>
              </div>
            ) : (
              <form onSubmit={handleSubmit} className="space-y-6">
                <p className="text-gray-600 text-sm">
                  Introduceți adresa de e-mail înregistrată, vă vom trimite un link de resetare a parolei.
                </p>

                {error && (
                  <div className="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-xl text-sm">
                    {error}
                  </div>
                )}

                <div className="space-y-2">
                  <label htmlFor="email" className="text-sm font-medium text-gray-700">
                    Adresă de e-mail
                  </label>
                  <div className="relative">
                    <Mail className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 h-5 w-5" />
                    <Input
                      id="email"
                      type="email"
                      required
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      className="pl-10 h-12 border-gray-200 focus:border-blue-500 focus:ring-blue-500 rounded-xl"
                      placeholder="exemplu@email.com"
                    />
                  </div>
                </div>

                <Button
                  type="submit"
                  disabled={isLoading}
                  className="w-full h-12 bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white font-semibold rounded-xl shadow-lg hover:shadow-xl transition-all duration-300"
                >
                  {isLoading ? (
                    <div className="flex items-center justify-center">
                      <div className="animate-spin rounded-full h-5 w-5 border-b-2 border-white mr-2"></div>
                      Se trimite...
                    </div>
                  ) : (
                    <div className="flex items-center justify-center">
                      Trimite link de resetare
                      <ArrowRight className="ml-2 h-5 w-5" />
                    </div>
                  )}
                </Button>

                <div className="text-center">
                  <Link href="/login" className="inline-flex items-center text-sm text-blue-600 hover:text-blue-700 font-medium transition-colors">
                    <ArrowLeft className="h-4 w-4 mr-1" />
                    Înapoi la pagina de autentificare
                  </Link>
                </div>
              </form>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
}
