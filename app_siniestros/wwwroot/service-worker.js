const CACHE = 'siniestros-v1';
const ASSETS = [
    '/',
    '/css/site.css', '/js/site.js',
    // agrega aquí rutas que quieras cachear desde el primer uso
];

self.addEventListener('install', (e) => {
    e.waitUntil(
        caches.open(CACHE)
            .then(c => c.addAll(ASSETS))
            .then(() => self.skipWaiting())
    );
});

self.addEventListener('activate', (e) => {
    e.waitUntil(
        caches.keys().then(keys =>
            Promise.all(keys.map(k => k !== CACHE && caches.delete(k)))
        )
    );
    self.clients.claim();
});

// Cache-first con fallback a red y página offline para navegaciones
self.addEventListener('fetch', (e) => {
    if (e.request.method !== 'GET') return;

    e.respondWith(
        caches.match(e.request).then(cacheRes => {
            if (cacheRes) return cacheRes;

            return fetch(e.request).then(netRes => {
                const copy = netRes.clone();
                caches.open(CACHE).then(c => c.put(e.request, copy));
                return netRes;
            }).catch(() => {
                if (e.request.mode === 'navigate') {
                    return caches.match('/offline.html');
                }
            });
        })
    );
});
