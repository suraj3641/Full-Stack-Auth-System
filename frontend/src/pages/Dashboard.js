import { useEffect, useState } from 'react';
import { FaSignOutAlt } from 'react-icons/fa';
import { useNavigate } from 'react-router-dom';
import { authService } from '../services/api';
import '../styles/global.css';
import { getUser, logout } from '../utils/auth';

const Dashboard = () => {
    const navigate = useNavigate();
    const [user, setUser] = useState(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        const fetchProfile = async () => {
            try {
                const response = await authService.getProfile();
                setUser(response.data);
            } catch (error) {
                console.error('Failed to fetch profile', error);
                logout();
                navigate('/login');
            } finally {
                setLoading(false);
            }
        };

        const storedUser = getUser();
        if (storedUser) {
            setUser(storedUser);
            setLoading(false);
            fetchProfile(); // Refresh from API
        } else {
            fetchProfile();
        }
    }, [navigate]);

    const handleLogout = () => {
        logout();
        navigate('/login');
    };

    if (loading) {
        return (
            <div className="auth-container">
                <div className="auth-card" style={{textAlign: 'center'}}>
                    <div className="loader"></div>
                    <p>Loading dashboard...</p>
                </div>
            </div>
        );
    }

    return (
        <div className="dashboard-container">
            <nav className="dashboard-nav">
                <div className="nav-brand">
                    🚀 AuthSystem
                </div>
                <div className="nav-user">
                    <div className="user-avatar">
                        {user?.name?.charAt(0).toUpperCase()}
                    </div>
                    <span>Welcome, {user?.name}</span>
                    <button onClick={handleLogout} className="logout-btn">
                        <FaSignOutAlt /> Logout
                    </button>
                </div>
            </nav>
            
            <div className="dashboard-content">
                <div className="welcome-card">
                    <h1>Welcome to Dashboard! 🎉</h1>
                    <p>You have successfully logged into your account.</p>
                    
                    <div className="user-info-grid">
                        <div className="info-card">
                            <div className="info-icon">👤</div>
                            <div className="info-label">Full Name</div>
                            <div className="info-value">{user?.name}</div>
                        </div>
                        
                        <div className="info-card">
                            <div className="info-icon">📧</div>
                            <div className="info-label">Email Address</div>
                            <div className="info-value">{user?.email}</div>
                        </div>
                        
                        <div className="info-card">
                            <div className="info-icon">🏷️</div>
                            <div className="info-label">Role</div>
                            <div className="info-value">
                                <span style={{
                                    background: user?.role === 'Admin' ? '#28a745' : '#667eea',
                                    color: 'white',
                                    padding: '4px 12px',
                                    borderRadius: '20px',
                                    fontSize: '12px'
                                }}>
                                    {user?.role || 'User'}
                                </span>
                            </div>
                        </div>
                        
                        <div className="info-card">
                            <div className="info-icon">📅</div>
                            <div className="info-label">Member Since</div>
                            <div className="info-value">
                                {new Date(user?.createdAt).toLocaleDateString()}
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Dashboard;