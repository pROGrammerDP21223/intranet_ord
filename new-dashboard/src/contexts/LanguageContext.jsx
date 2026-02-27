import React, { createContext, useContext, useState, useEffect } from 'react';

const LanguageContext = createContext();

const translations = {
  en: {
    // Common
    'common.save': 'Save',
    'common.cancel': 'Cancel',
    'common.delete': 'Delete',
    'common.edit': 'Edit',
    'common.create': 'Create',
    'common.search': 'Search',
    'common.loading': 'Loading...',
    'common.error': 'Error',
    'common.success': 'Success',
    'common.confirm': 'Confirm',
    'common.close': 'Close',
    
    // Navigation
    'nav.dashboard': 'Dashboard',
    'nav.clients': 'Clients',
    'nav.products': 'Products',
    'nav.users': 'Users',
    'nav.services': 'Services',
    'nav.enquiries': 'Enquiries',
    'nav.tickets': 'Tickets',
    'nav.analytics': 'Analytics',
    'nav.tools': 'Internal Messaging',
    'nav.settings': 'Settings',
    
    // Auth
    'auth.login': 'Login',
    'auth.logout': 'Logout',
    'auth.register': 'Register',
    'auth.email': 'Email',
    'auth.password': 'Password',
    'auth.forgotPassword': 'Forgot Password?',
    
    // Dashboard
    'dashboard.welcome': 'Welcome',
    'dashboard.totalClients': 'Total Clients',
    'dashboard.totalEnquiries': 'Total Enquiries',
    'dashboard.totalTickets': 'Total Tickets',
    'dashboard.totalTransactions': 'Total Transactions',
    
    // Backup & Archive
    'backup.title': 'Backup Management',
    'backup.create': 'Create Backup',
    'backup.restore': 'Restore',
    'backup.delete': 'Delete',
    'backup.schedule': 'Schedule Backup',
    'archive.title': 'Archive Management',
    'archive.archive': 'Archive Data',
    'archive.schedule': 'Schedule Archiving',
    'archive.restore': 'Restore',
  },
  es: {
    // Common
    'common.save': 'Guardar',
    'common.cancel': 'Cancelar',
    'common.delete': 'Eliminar',
    'common.edit': 'Editar',
    'common.create': 'Crear',
    'common.search': 'Buscar',
    'common.loading': 'Cargando...',
    'common.error': 'Error',
    'common.success': 'Éxito',
    'common.confirm': 'Confirmar',
    'common.close': 'Cerrar',
    
    // Navigation
    'nav.dashboard': 'Panel',
    'nav.clients': 'Clientes',
    'nav.products': 'Productos',
    'nav.users': 'Usuarios',
    'nav.services': 'Servicios',
    'nav.enquiries': 'Consultas',
    'nav.tickets': 'Tickets',
    'nav.analytics': 'Analíticas',
    'nav.tools': 'Mensajería Interna',
    'nav.settings': 'Configuración',
    
    // Auth
    'auth.login': 'Iniciar Sesión',
    'auth.logout': 'Cerrar Sesión',
    'auth.register': 'Registrarse',
    'auth.email': 'Correo',
    'auth.password': 'Contraseña',
    'auth.forgotPassword': '¿Olvidaste tu Contraseña?',
    
    // Dashboard
    'dashboard.welcome': 'Bienvenido',
    'dashboard.totalClients': 'Total Clientes',
    'dashboard.totalEnquiries': 'Total Consultas',
    'dashboard.totalTickets': 'Total Tickets',
    'dashboard.totalTransactions': 'Total Transacciones',
    
    // Backup & Archive
    'backup.title': 'Gestión de Copias de Seguridad',
    'backup.create': 'Crear Copia de Seguridad',
    'backup.restore': 'Restaurar',
    'backup.delete': 'Eliminar',
    'backup.schedule': 'Programar Copia de Seguridad',
    'archive.title': 'Gestión de Archivos',
    'archive.archive': 'Archivar Datos',
    'archive.schedule': 'Programar Archivado',
    'archive.restore': 'Restaurar',
  },
  fr: {
    // Common
    'common.save': 'Enregistrer',
    'common.cancel': 'Annuler',
    'common.delete': 'Supprimer',
    'common.edit': 'Modifier',
    'common.create': 'Créer',
    'common.search': 'Rechercher',
    'common.loading': 'Chargement...',
    'common.error': 'Erreur',
    'common.success': 'Succès',
    'common.confirm': 'Confirmer',
    'common.close': 'Fermer',
    
    // Navigation
    'nav.dashboard': 'Tableau de bord',
    'nav.clients': 'Clients',
    'nav.products': 'Produits',
    'nav.users': 'Utilisateurs',
    'nav.services': 'Services',
    'nav.enquiries': 'Demandes',
    'nav.tickets': 'Tickets',
    'nav.analytics': 'Analyses',
    'nav.tools': 'Messagerie Interne',
    'nav.settings': 'Paramètres',
    
    // Auth
    'auth.login': 'Connexion',
    'auth.logout': 'Déconnexion',
    'auth.register': 'S\'inscrire',
    'auth.email': 'Email',
    'auth.password': 'Mot de passe',
    'auth.forgotPassword': 'Mot de passe oublié?',
    
    // Dashboard
    'dashboard.welcome': 'Bienvenue',
    'dashboard.totalClients': 'Total Clients',
    'dashboard.totalEnquiries': 'Total Demandes',
    'dashboard.totalTickets': 'Total Tickets',
    'dashboard.totalTransactions': 'Total Transactions',
    
    // Backup & Archive
    'backup.title': 'Gestion des Sauvegardes',
    'backup.create': 'Créer une Sauvegarde',
    'backup.restore': 'Restaurer',
    'backup.delete': 'Supprimer',
    'backup.schedule': 'Planifier une Sauvegarde',
    'archive.title': 'Gestion des Archives',
    'archive.archive': 'Archiver les Données',
    'archive.schedule': 'Planifier l\'Archivage',
    'archive.restore': 'Restaurer',
  },
};

export const useLanguage = () => {
  return useContext(LanguageContext);
};

export const LanguageProvider = ({ children }) => {
  const [language, setLanguage] = useState(() => {
    const saved = localStorage.getItem('language');
    return saved || 'en';
  });

  useEffect(() => {
    localStorage.setItem('language', language);
    document.documentElement.setAttribute('lang', language);
  }, [language]);

  const t = (key) => {
    return translations[language]?.[key] || key;
  };

  const value = {
    language,
    setLanguage,
    t,
    availableLanguages: ['en', 'es', 'fr'],
  };

  return (
    <LanguageContext.Provider value={value}>
      {children}
    </LanguageContext.Provider>
  );
};

