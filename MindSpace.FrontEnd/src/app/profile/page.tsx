'use client';

import { useState, useEffect } from 'react';
import { useAuth } from '@/contexts/AuthContext';
import { useRouter } from 'next/navigation';
import Button from '@/components/ui/Button';
import Input from '@/components/ui/Input';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/Card';
import { PostCard } from '@/components/PostCard';
import { 
  User, 
  Mail, 
  Calendar, 
  MapPin, 
  Link as LinkIcon, 
  Twitter, 
  Linkedin,
  Edit3,
  Settings,
  BookOpen,
  Heart,
  Eye,
  Users,
  Award
} from 'lucide-react';
import Link from 'next/link';
import { Post } from '@/types';
import { postsService } from '@/services/posts';
import { usersService, FollowerUser, FollowersResponse, FollowingResponse } from '@/services/users';
import { X, Save } from 'lucide-react';

export default function ProfilePage() {
  const { user, isAuthenticated, updateUser } = useAuth();
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<'posts' | 'drafts' | 'liked' | 'followers' | 'following'>('posts');
  const [posts, setPosts] = useState<Post[]>([]);
  const [followers, setFollowers] = useState<FollowerUser[]>([]);
  const [following, setFollowing] = useState<FollowerUser[]>([]);
  const [loading, setLoading] = useState(true);
  const [followersLoading, setFollowersLoading] = useState(false);
  const [followingLoading, setFollowingLoading] = useState(false);
  const [isEditing, setIsEditing] = useState(false);
  const [editLoading, setEditLoading] = useState(false);
  const [editError, setEditError] = useState('');
  const [editSuccess, setEditSuccess] = useState('');
  const [editForm, setEditForm] = useState({
    firstName: '',
    lastName: '',
    bio: '',
    profileImageUrl: '',
    website: '',
    twitterHandle: '',
    linkedInUrl: ''
  });

  // Redirect if not authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      router.push('/login');
      return;
    }
  }, [isAuthenticated, router]);

  // Fetch user posts
  useEffect(() => {
    const fetchUserPosts = async () => {
      if (!user) return;
      
      try {
        setLoading(true);
        const postsResult = await postsService.getMyPosts(1, 10);
        setPosts(postsResult.items || []);
      } catch (error: unknown) {
        console.error('Error fetching user posts:', error);
        setPosts([]); // În caz de eroare, array gol
      } finally {
        setLoading(false);
      }
    };

    fetchUserPosts();
  }, [user]);

  // Fetch followers
  const fetchFollowers = async () => {
    if (!user) return;
    
    try {
      setFollowersLoading(true);
      const response: FollowersResponse = await usersService.getFollowers(user.userName, 1, 50);
      setFollowers(response.followers);
    } catch (error: unknown) {
      console.error('Error fetching followers:', error);
      setFollowers([]);
    } finally {
      setFollowersLoading(false);
    }
  };

  // Fetch following
  const fetchFollowing = async () => {
    if (!user) return;
    
    try {
      setFollowingLoading(true);
      const response: FollowingResponse = await usersService.getFollowing(user.userName, 1, 50);
      setFollowing(response.following);
    } catch (error: unknown) {
      console.error('Error fetching following:', error);
      setFollowing([]);
    } finally {
      setFollowingLoading(false);
    }
  };

  // Tab değiştiğinde ilgili veriyi yükle
  useEffect(() => {
    if (activeTab === 'followers' && followers.length === 0) {
      fetchFollowers();
    } else if (activeTab === 'following' && following.length === 0) {
      fetchFollowing();
    }
  }, [activeTab, user]);

  if (!user) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>
    );
  }

  const stats = [
    { label: 'Articole', value: posts.length, icon: BookOpen, color: 'text-blue-600' },
    { label: 'Urmăritori', value: user.followerCount || 0, icon: Users, color: 'text-green-600' },
    { label: 'Urmăriri', value: user.followingCount || 0, icon: Heart, color: 'text-red-600' },
    { label: 'Vizualizări', value: posts.reduce((acc, post) => acc + (post.viewCount || 0), 0), icon: Eye, color: 'text-purple-600' }
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-purple-50 to-indigo-100">
      <div className="container mx-auto px-4 py-8">
        {/* Profile Header */}
        <Card className="backdrop-blur-xl bg-white/80 shadow-2xl border-0 rounded-2xl mb-8">
          <CardContent className="p-8">
            <div className="flex flex-col lg:flex-row items-start gap-8">
              {/* Avatar & Basic Info */}
              <div className="flex-shrink-0">
                <div className="w-32 h-32 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center text-white text-4xl font-bold shadow-xl">
                  {user.firstName?.[0]}{user.lastName?.[0]}
                </div>
                {user.isVerified && (
                  <div className="flex items-center justify-center mt-3">
                    <Award className="h-5 w-5 text-blue-500 mr-1" />
                    <span className="text-sm text-blue-600 font-medium">Verificat</span>
                  </div>
                )}
              </div>

              {/* User Details */}
              <div className="flex-1 min-w-0">
                <div className="flex items-center justify-between mb-4">
                  <div>
                    <h1 className="text-3xl font-bold text-gray-900 mb-1">
                      {user.firstName} {user.lastName}
                    </h1>
                    <p className="text-gray-600 text-lg">@{user.userName}</p>
                  </div>
                  
                  <div className="flex gap-3">
                    <Button
                      onClick={() => {
                        setEditForm({
                          firstName: user.firstName || '',
                          lastName: user.lastName || '',
                          bio: user.bio || '',
                          profileImageUrl: user.profileImageUrl || '',
                          website: user.website || '',
                          twitterHandle: user.twitterHandle || '',
                          linkedInUrl: user.linkedInUrl || ''
                        });
                        setEditError('');
                        setEditSuccess('');
                        setIsEditing(true);
                      }}
                      variant="outline"
                      className="border-gray-300 hover:border-blue-500 rounded-full"
                    >
                      <Edit3 className="h-4 w-4 mr-2" />
                      Editează
                    </Button>
                    
                    <Button
                      variant="outline"
                      className="border-gray-300 hover:border-blue-500 rounded-full"
                    >
                      <Settings className="h-4 w-4 mr-2" />
                      Setări
                    </Button>
                  </div>
                </div>

                {/* Bio */}
                {user.bio && (
                  <p className="text-gray-700 mb-6 leading-relaxed">
                    {user.bio}
                  </p>
                )}

                {/* Contact Info */}
                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                  <div className="flex items-center text-gray-600">
                    <Mail className="h-5 w-5 mr-3 text-gray-400" />
                    <span>{user.email}</span>
                  </div>
                  
                  <div className="flex items-center text-gray-600">
                    <Calendar className="h-5 w-5 mr-3 text-gray-400" />
                    <span>Înscris: {new Date(user.joinDate).toLocaleDateString('ro-RO')}</span>
                  </div>
                  
                  {user.website && (
                    <div className="flex items-center text-gray-600">
                      <LinkIcon className="h-5 w-5 mr-3 text-gray-400" />
                      <a href={user.website} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:text-blue-700">
                        {user.website}
                      </a>
                    </div>
                  )}
                  
                  {user.twitterHandle && (
                    <div className="flex items-center text-gray-600">
                      <Twitter className="h-5 w-5 mr-3 text-gray-400" />
                      <a href={`https://twitter.com/${user.twitterHandle}`} target="_blank" rel="noopener noreferrer" className="text-blue-600 hover:text-blue-700">
                        @{user.twitterHandle}
                      </a>
                    </div>
                  )}
                </div>

                {/* Stats */}
                <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
                  {stats.map((stat, index) => (
                    <div key={index} className="text-center p-4 bg-white/60 rounded-xl">
                      <stat.icon className={`h-6 w-6 mx-auto mb-2 ${stat.color}`} />
                      <div className="text-2xl font-bold text-gray-900">{stat.value}</div>
                      <div className="text-sm text-gray-600">{stat.label}</div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </CardContent>
        </Card>



        {/* Edit Profile Modal */}
        {isEditing && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
            <Card className="w-full max-w-lg bg-white shadow-2xl border-0 rounded-2xl max-h-[90vh] overflow-y-auto">
              <CardHeader className="border-b border-gray-100 sticky top-0 bg-white rounded-t-2xl z-10">
                <div className="flex items-center justify-between">
                  <CardTitle className="text-xl font-bold text-gray-900">Editează profilul</CardTitle>
                  <button
                    onClick={() => setIsEditing(false)}
                    className="p-2 hover:bg-gray-100 rounded-full transition-colors"
                  >
                    <X className="h-5 w-5 text-gray-500" />
                  </button>
                </div>
              </CardHeader>
              <CardContent className="p-6">
                {editError && (
                  <div className="bg-red-50 border border-red-200 text-red-600 px-4 py-3 rounded-xl text-sm mb-4">
                    {editError}
                  </div>
                )}
                {editSuccess && (
                  <div className="bg-green-50 border border-green-200 text-green-600 px-4 py-3 rounded-xl text-sm mb-4">
                    {editSuccess}
                  </div>
                )}
                <form
                  onSubmit={async (e) => {
                    e.preventDefault();
                    setEditLoading(true);
                    setEditError('');
                    setEditSuccess('');
                    try {
                      const updated = await usersService.updateProfile(editForm);
                      updateUser({ ...user!, ...updated });
                      setEditSuccess('Profilul a fost actualizat cu succes');
                      setTimeout(() => setIsEditing(false), 1200);
                    } catch {
                      setEditError('A apărut o eroare la actualizarea profilului');
                    } finally {
                      setEditLoading(false);
                    }
                  }}
                  className="space-y-4"
                >
                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1">
                      <label className="text-sm font-medium text-gray-700">Prenume</label>
                      <Input
                        value={editForm.firstName}
                        onChange={(e) => setEditForm(p => ({ ...p, firstName: e.target.value }))}
                        className="h-11 border-gray-200 rounded-xl"
                        placeholder="Prenumele dvs."
                      />
                    </div>
                    <div className="space-y-1">
                      <label className="text-sm font-medium text-gray-700">Nume</label>
                      <Input
                        value={editForm.lastName}
                        onChange={(e) => setEditForm(p => ({ ...p, lastName: e.target.value }))}
                        className="h-11 border-gray-200 rounded-xl"
                        placeholder="Numele dvs."
                      />
                    </div>
                  </div>

                  <div className="space-y-1">
                    <label className="text-sm font-medium text-gray-700">Biografie</label>
                    <textarea
                      value={editForm.bio}
                      onChange={(e) => setEditForm(p => ({ ...p, bio: e.target.value }))}
                      rows={3}
                      className="w-full px-3 py-2 border border-gray-200 rounded-xl text-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent resize-none"
                      placeholder="Scrieți ceva scurt despre dvs..."
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-sm font-medium text-gray-700">URL poză de profil</label>
                    <Input
                      value={editForm.profileImageUrl}
                      onChange={(e) => setEditForm(p => ({ ...p, profileImageUrl: e.target.value }))}
                      className="h-11 border-gray-200 rounded-xl"
                      placeholder="https://..."
                    />
                  </div>

                  <div className="space-y-1">
                    <label className="text-sm font-medium text-gray-700">Website</label>
                    <Input
                      value={editForm.website}
                      onChange={(e) => setEditForm(p => ({ ...p, website: e.target.value }))}
                      className="h-11 border-gray-200 rounded-xl"
                      placeholder="https://website.com"
                    />
                  </div>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="space-y-1">
                      <label className="text-sm font-medium text-gray-700">Twitter</label>
                      <Input
                        value={editForm.twitterHandle}
                        onChange={(e) => setEditForm(p => ({ ...p, twitterHandle: e.target.value }))}
                        className="h-11 border-gray-200 rounded-xl"
                        placeholder="@numeutilizator"
                      />
                    </div>
                    <div className="space-y-1">
                      <label className="text-sm font-medium text-gray-700">LinkedIn URL</label>
                      <Input
                        value={editForm.linkedInUrl}
                        onChange={(e) => setEditForm(p => ({ ...p, linkedInUrl: e.target.value }))}
                        className="h-11 border-gray-200 rounded-xl"
                        placeholder="linkedin.com/in/..."
                      />
                    </div>
                  </div>

                  <div className="flex gap-3 pt-2">
                    <Button
                      type="button"
                      variant="outline"
                      onClick={() => setIsEditing(false)}
                      className="flex-1 h-11 border-gray-300 rounded-xl"
                    >
                      Anulați
                    </Button>
                    <Button
                      type="submit"
                      disabled={editLoading}
                      className="flex-1 h-11 bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white rounded-xl"
                    >
                      {editLoading ? (
                        <div className="flex items-center justify-center">
                          <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                          Se salvează...
                        </div>
                      ) : (
                        <div className="flex items-center justify-center">
                          <Save className="h-4 w-4 mr-2" />
                          Salvează
                        </div>
                      )}
                    </Button>
                  </div>
                </form>
              </CardContent>
            </Card>
          </div>
        )}

        {/* Content Tabs */}
        <Card className="backdrop-blur-xl bg-white/80 shadow-xl border-0 rounded-2xl">
          <CardHeader className="border-b border-gray-100">
            <div className="flex space-x-8">
              {[
                { key: 'posts', label: 'Articolele mele', count: posts.length },
                { key: 'followers', label: 'Urmăritori', count: user.followerCount || 0 },
                { key: 'following', label: 'Urmăriri', count: user.followingCount || 0 },
                { key: 'drafts', label: 'Ciorne', count: 0 },
                { key: 'liked', label: 'Aprecieri', count: 0 }
              ].map((tab) => (
                <button
                  key={tab.key}
                  onClick={() => setActiveTab(tab.key as 'posts' | 'drafts' | 'liked')}
                  className={`flex items-center space-x-2 px-4 py-2 rounded-full transition-all duration-300 ${
                    activeTab === tab.key
                      ? 'bg-gradient-to-r from-blue-600 to-purple-600 text-white shadow-lg'
                      : 'text-gray-600 hover:text-blue-600 hover:bg-blue-50'
                  }`}
                >
                  <span className="font-medium">{tab.label}</span>
                  {tab.count > 0 && (
                    <span className={`text-xs px-2 py-1 rounded-full ${
                      activeTab === tab.key ? 'bg-white/20' : 'bg-gray-200'
                    }`}>
                      {tab.count}
                    </span>
                  )}
                </button>
              ))}
            </div>
          </CardHeader>

          <CardContent className="p-8">
            {loading ? (
              <div className="text-center py-12">
                <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
                <p className="text-gray-600">Se încarcă...</p>
              </div>
            ) : (
              <>
                {activeTab === 'posts' && (
                  <div className="space-y-6">
                    {posts.length === 0 ? (
                      <div className="text-center py-16">
                        <BookOpen className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                        <h3 className="text-xl font-semibold text-gray-900 mb-2">Nu aveți articole încă</h3>
                        <p className="text-gray-600 mb-6">Începeți prin a partaja primul dvs. articol!</p>
                        <Link href="/write">
                          <Button className="bg-gradient-to-r from-blue-600 to-purple-600 text-white hover:from-blue-700 hover:to-purple-700 rounded-full">
                            <Edit3 className="h-4 w-4 mr-2" />
                            Scrieți un articol
                          </Button>
                        </Link>
                      </div>
                    ) : (
                      posts.map((post) => (
                        <PostCard 
                          key={post.id} 
                          post={post} 
                          variant="default" 
                          showActions={true}
                          onEdit={(post) => {
                            // Redirecționare către pagina de editare
                            window.location.href = `/write?edit=${post.id}`;
                          }}
                          onDelete={(postId) => {
                            // Post'u listeden kaldır
                            setPosts(posts.filter(p => p.id !== postId));
                          }}
                        />
                      ))
                    )}
                  </div>
                )}

                {activeTab === 'drafts' && (
                  <div className="text-center py-16">
                    <Edit3 className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                    <h3 className="text-xl font-semibold text-gray-900 mb-2">Nu aveți ciorne</h3>
                    <p className="text-gray-600">Articolele dvs. în curs de scriere vor apărea aici.</p>
                  </div>
                )}

                {activeTab === 'followers' && (
                  <div className="space-y-6">
                    {followersLoading ? (
                      <div className="text-center py-12">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
                        <p className="text-gray-600">Se încarcă urmăritorii...</p>
                      </div>
                    ) : followers.length === 0 ? (
                      <div className="text-center py-16">
                        <Users className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                        <h3 className="text-xl font-semibold text-gray-900 mb-2">Nu aveți urmăritori încă</h3>
                        <p className="text-gray-600">Începeți să câștigați urmăritori partajând articolele dvs.!</p>
                      </div>
                    ) : (
                      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {followers.map((follower) => (
                          <Card key={follower.id} className="bg-white shadow-md hover:shadow-xl transition-all duration-300 border border-gray-100 hover:border-blue-200">
                            <CardContent className="p-6">
                              <div className="flex items-start space-x-4 mb-4">
                                <div className="w-16 h-16 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center text-white text-xl font-bold shadow-lg">
                                  {follower.firstName?.[0]}{follower.lastName?.[0]}
                                </div>
                                <div className="flex-1">
                                  <div className="flex items-center gap-2 mb-1">
                                    <h3 className="text-lg font-bold text-gray-900">
                                      {follower.firstName} {follower.lastName}
                                    </h3>
                                    {follower.isVerified && (
                                      <Award className="h-4 w-4 text-blue-500" />
                                    )}
                                  </div>
                                  <p className="text-sm text-gray-500 mb-2">@{follower.userName}</p>
                                  {follower.bio && (
                                    <p className="text-sm text-gray-600 line-clamp-2">
                                      {follower.bio}
                                    </p>
                                  )}
                                </div>
                              </div>
                              
                              <div className="grid grid-cols-2 gap-4 mb-4">
                                <div className="text-center">
                                  <div className="flex items-center justify-center gap-1 text-blue-600 mb-1">
                                    <BookOpen className="h-4 w-4" />
                                    <span className="text-sm font-semibold">{follower.followerCount}</span>
                                  </div>
                                  <p className="text-xs text-gray-500">Urmăritori</p>
                                </div>
                                <div className="text-center">
                                  <div className="flex items-center justify-center gap-1 text-green-600 mb-1">
                                    <Heart className="h-4 w-4" />
                                    <span className="text-sm font-semibold">{follower.followingCount}</span>
                                  </div>
                                  <p className="text-xs text-gray-500">Urmăriri</p>
                                </div>
                              </div>

                              <Link href={`/profile/${follower.userName}`}>
                                <Button 
                                  size="sm" 
                                  className="w-full bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white rounded-lg"
                                >
                                  Vezi profilul
                                </Button>
                              </Link>
                            </CardContent>
                          </Card>
                        ))}
                      </div>
                    )}
                  </div>
                )}

                {activeTab === 'following' && (
                  <div className="space-y-6">
                    {followingLoading ? (
                      <div className="text-center py-12">
                        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600 mx-auto mb-4"></div>
                        <p className="text-gray-600">Se încarcă urmăririle...</p>
                      </div>
                    ) : following.length === 0 ? (
                      <div className="text-center py-16">
                        <Heart className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                        <h3 className="text-xl font-semibold text-gray-900 mb-2">Nu urmăriți pe nimeni încă</h3>
                        <p className="text-gray-600">Începeți să urmăriți autorii care vă interesează!</p>
                        <Link href="/writers" className="inline-block mt-4">
                          <Button className="bg-gradient-to-r from-blue-600 to-purple-600 text-white hover:from-blue-700 hover:to-purple-700 rounded-full">
                            Descoperă autori
                          </Button>
                        </Link>
                      </div>
                    ) : (
                      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                        {following.map((followedUser) => (
                          <Card key={followedUser.id} className="bg-white shadow-md hover:shadow-xl transition-all duration-300 border border-gray-100 hover:border-blue-200">
                            <CardContent className="p-6">
                              <div className="flex items-start space-x-4 mb-4">
                                <div className="w-16 h-16 bg-gradient-to-r from-blue-500 to-purple-500 rounded-full flex items-center justify-center text-white text-xl font-bold shadow-lg">
                                  {followedUser.firstName?.[0]}{followedUser.lastName?.[0]}
                                </div>
                                <div className="flex-1">
                                  <div className="flex items-center gap-2 mb-1">
                                    <h3 className="text-lg font-bold text-gray-900">
                                      {followedUser.firstName} {followedUser.lastName}
                                    </h3>
                                    {followedUser.isVerified && (
                                      <Award className="h-4 w-4 text-blue-500" />
                                    )}
                                  </div>
                                  <p className="text-sm text-gray-500 mb-2">@{followedUser.userName}</p>
                                  {followedUser.bio && (
                                    <p className="text-sm text-gray-600 line-clamp-2">
                                      {followedUser.bio}
                                    </p>
                                  )}
                                </div>
                              </div>
                              
                              <div className="grid grid-cols-2 gap-4 mb-4">
                                <div className="text-center">
                                  <div className="flex items-center justify-center gap-1 text-blue-600 mb-1">
                                    <BookOpen className="h-4 w-4" />
                                    <span className="text-sm font-semibold">{followedUser.followerCount}</span>
                                  </div>
                                  <p className="text-xs text-gray-500">Urmăritori</p>
                                </div>
                                <div className="text-center">
                                  <div className="flex items-center justify-center gap-1 text-green-600 mb-1">
                                    <Heart className="h-4 w-4" />
                                    <span className="text-sm font-semibold">{followedUser.followingCount}</span>
                                  </div>
                                  <p className="text-xs text-gray-500">Urmăriri</p>
                                </div>
                              </div>

                              <Link href={`/profile/${followedUser.userName}`}>
                                <Button 
                                  size="sm" 
                                  className="w-full bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white rounded-lg"
                                >
                                  Vezi profilul
                                </Button>
                              </Link>
                            </CardContent>
                          </Card>
                        ))}
                      </div>
                    )}
                  </div>
                )}

                {activeTab === 'liked' && (
                  <div className="text-center py-16">
                    <Heart className="h-16 w-16 text-gray-300 mx-auto mb-4" />
                    <h3 className="text-xl font-semibold text-gray-900 mb-2">Nu ați apreciat niciun articol încă</h3>
                    <p className="text-gray-600">Articolele apreciate vor apărea aici.</p>
                  </div>
                )}
              </>
            )}
          </CardContent>
        </Card>
      </div>
    </div>
  );
} 