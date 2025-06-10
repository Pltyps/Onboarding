import React, { createContext, useState, useEffect, useContext } from 'react';

export type UserRole = 'Admin' | 'FullTime' | 'Student' | null;

interface User {
  email: string;
  role: UserRole;
  department: string;
}

interface AuthContextType {
  user: User | null;
  isLoggedIn: boolean;
  login: (email: string, role: UserRole, department: string) => void;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType & { loading: boolean }>({
  user: null,
  isLoggedIn: false,
  login: () => {},
  logout: () => {},
  loading: true,
});

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({
  children,
}) => {
  const [user, setUser] = useState<User | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem('user');
    if (stored) {
      setUser(JSON.parse(stored));
    }
    setLoading(false);
  }, []);

  const login = (email: string, role: UserRole, department: string) => {
    const newUser = { email, role, department };
    setUser(newUser);
    localStorage.setItem('user', JSON.stringify(newUser));
  };

  const logout = () => {
    setUser(null);
    localStorage.removeItem('user');
  };

  return (
    <AuthContext.Provider
      value={{ user, isLoggedIn: !!user, login, logout, loading }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
