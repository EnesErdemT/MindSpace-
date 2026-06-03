'use client';
import { useState, useEffect, use } from 'react';
import { useRouter } from 'next/navigation';
import { useAuth } from '@/contexts/AuthContext';
import { postsService } from '@/services/posts';
import { categoriesService } from '@/services/categories';
import { tagsService } from '@/services/tags';
import { Post, Category, Tag } from '@/types';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { ArrowLeft, Save, Loader2 } from 'lucide-react';

interface EditPostPageProps {
  params: Promise<{
    slug: string;
  }>;
}

export default function EditPostPage({ params }: EditPostPageProps) {
  const { slug } = use(params);
  const router = useRouter();
  const { isAuthenticated, user, isLoading } = useAuth();
  const [post, setPost] = useState<Post | null>(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [categories, setCategories] = useState<Category[]>([]);
  const [tags, setTags] = useState<Tag[]>([]);
  const [formData, setFormData] = useState({
    title: '',
    content: '',
    excerpt: '',
    featuredImageUrl: '',
    categoryId: '',
    tagIds: [] as string[]
  });

  useEffect(() => {
    console.log('EditPost useEffect:', { 
      isAuthenticated, 
      user: user?.id, 
      userName: user?.userName,
      slug: slug,
      userExists: !!user,
      isLoading
    });
    
    // AuthContext henüz yüklenmediyse bekle
    if (isLoading || isAuthenticated === undefined) {
      console.log('AuthContext still loading...');
      return;
    }
    
    if (!isAuthenticated) {
      console.log('Not authenticated, redirecting to login');
      router.push('/login');
      return;
    }

    if (user) {
      console.log('User authenticated, loading post data');
      loadPost();
      loadCategories();
      loadTags();
    } else {
      console.log('User is null, waiting for user data...');
    }
  }, [isAuthenticated, user, slug, isLoading]);

  const loadPost = async () => {
    try {
      setLoading(true);
      console.log('Loading post data for slug:', slug);
      const postData = await postsService.getPostBySlug(slug);
      console.log('Post data loaded:', { 
        postId: postData.id, 
        authorId: postData.author.id, 
        currentUserId: user?.id,
        authorMatch: postData.author.id === user?.id
      });
      
      // Post sahibi kontrolü
      if (!user || postData.author.id !== user.id) {
        console.log('User is not the author, redirecting to home');
        console.log('Author ID:', postData.author.id);
        console.log('User ID:', user?.id);
        console.log('Match:', postData.author.id === user?.id);
        router.push('/');
        return;
      }

      setPost(postData);
      setFormData({
        title: postData.title,
        content: postData.content,
        excerpt: postData.excerpt || '',
        featuredImageUrl: postData.featuredImageUrl || '',
        categoryId: postData.category?.id || '',
        tagIds: postData.tags?.map(tag => tag.id) || []
      });
    } catch (error) {
      console.error('Eroare la încărcarea articolului:', error);
      router.push('/');
    } finally {
      setLoading(false);
    }
  };

  const loadCategories = async () => {
    try {
      const response = await categoriesService.getCategories();
      setCategories(response || []);
    } catch (error) {
      console.error('Eroare la încărcarea categoriilor:', error);
    }
  };

  const loadTags = async () => {
    try {
      const response = await tagsService.getTags();
      setTags(response || []);
    } catch (error) {
      console.error('Eroare la încărcarea etichetelor:', error);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!post) return;

    try {
      setSaving(true);
      
      await postsService.updatePost(post.id, {
        title: formData.title,
        content: formData.content,
        excerpt: formData.excerpt,
        featuredImageUrl: formData.featuredImageUrl || null,
        categoryId: formData.categoryId || null,
        tags: formData.tagIds,
        status: post.status
      });

      router.push(`/post/${post.slug}`);
    } catch (error) {
      console.error('Eroare la actualizarea articolului:', error);
    } finally {
      setSaving(false);
    }
  };

  const handleTagToggle = (tagId: string) => {
    setFormData(prev => ({
      ...prev,
      tagIds: prev.tagIds.includes(tagId)
        ? prev.tagIds.filter(id => id !== tagId)
        : [...prev.tagIds, tagId]
    }));
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
        <div className="flex items-center gap-2">
          <Loader2 className="h-6 w-6 animate-spin" />
          <span>Se verifică autentificarea...</span>
        </div>
      </div>
    );
  }

  if (!isAuthenticated || !user) {
    return null; // Router zaten login'e yönlendirecek
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
        <div className="flex items-center gap-2">
          <Loader2 className="h-6 w-6 animate-spin" />
          <span>Se încarcă...</span>
        </div>
      </div>
    );
  }

  if (!post) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center">
        <div className="text-center">
          <h1 className="text-2xl font-bold text-gray-900 mb-2">Articolul nu a fost găsit</h1>
          <p className="text-gray-600 mb-4">Articolul căutat nu există sau nu aveți permisiunea de a-l accesa.</p>
          <Button onClick={() => router.push('/')}>
            <ArrowLeft className="h-4 w-4 mr-2" />
            Înapoi la pagina principală
          </Button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 py-8">
      <div className="max-w-4xl mx-auto px-4">
        {/* Header */}
        <div className="flex items-center gap-4 mb-8">
          <Button
            variant="outline"
            onClick={() => router.push(`/post/${post.slug}`)}
            className="flex items-center gap-2"
          >
            <ArrowLeft className="h-4 w-4" />
            Înapoi
          </Button>
          <h1 className="text-3xl font-bold text-gray-900">Editează Articolul</h1>
        </div>

        <form onSubmit={handleSubmit} className="space-y-6">
          {/* Title */}
          <Card>
            <CardHeader>
              <CardTitle>Titlu</CardTitle>
            </CardHeader>
            <CardContent>
              <Input
                value={formData.title}
                onChange={(e) => setFormData(prev => ({ ...prev, title: e.target.value }))}
                placeholder="Introduceți titlul articolului..."
                required
                className="text-lg"
              />
            </CardContent>
          </Card>

          {/* Content */}
          <Card>
            <CardHeader>
              <CardTitle>Conținut</CardTitle>
            </CardHeader>
            <CardContent>
              <textarea
                value={formData.content}
                onChange={(e) => setFormData(prev => ({ ...prev, content: e.target.value }))}
                placeholder="Scrieți conținutul articolului..."
                required
                rows={15}
                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
              />
            </CardContent>
          </Card>

          {/* Excerpt */}
          <Card>
            <CardHeader>
              <CardTitle>Rezumat</CardTitle>
            </CardHeader>
            <CardContent>
              <textarea
                value={formData.excerpt}
                onChange={(e) => setFormData(prev => ({ ...prev, excerpt: e.target.value }))}
                placeholder="Scrieți rezumatul articolului..."
                rows={3}
                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
              />
            </CardContent>
          </Card>

          {/* Featured Image */}
          <Card>
            <CardHeader>
              <CardTitle>URL Imagine Reprezentativă</CardTitle>
            </CardHeader>
            <CardContent>
              <Input
                value={formData.featuredImageUrl}
                onChange={(e) => setFormData(prev => ({ ...prev, featuredImageUrl: e.target.value }))}
                placeholder="https://example.com/image.jpg"
                type="url"
              />
            </CardContent>
          </Card>

          {/* Category */}
          <Card>
            <CardHeader>
              <CardTitle>Categorie</CardTitle>
            </CardHeader>
            <CardContent>
              <select
                value={formData.categoryId}
                onChange={(e) => setFormData(prev => ({ ...prev, categoryId: e.target.value }))}
                className="w-full p-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
              >
                <option value="">Selectați categoria...</option>
                {categories.map(category => (
                  <option key={category.id} value={category.id}>
                    {category.name}
                  </option>
                ))}
              </select>
            </CardContent>
          </Card>

          {/* Tags */}
          <Card>
            <CardHeader>
              <CardTitle>Etichete</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-2">
                {tags.map(tag => (
                  <label key={tag.id} className="flex items-center gap-2 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={formData.tagIds.includes(tag.id)}
                      onChange={() => handleTagToggle(tag.id)}
                      className="rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                    />
                    <span className="text-sm">{tag.name}</span>
                  </label>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* Submit Button */}
          <div className="flex justify-end gap-4">
            <Button
              type="button"
              variant="outline"
              onClick={() => router.push(`/post/${post.slug}`)}
            >
              Anulează
            </Button>
            <Button type="submit" disabled={saving}>
              {saving ? (
                <>
                  <Loader2 className="h-4 w-4 mr-2 animate-spin" />
                  Se salvează...
                </>
              ) : (
                <>
                  <Save className="h-4 w-4 mr-2" />
                  Salvează
                </>
              )}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
} 