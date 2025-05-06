let musicLibrary = [];
let currentAlbum = [];
let currentIndex = 0;

document.getElementById("fileInput").addEventListener("change", async () => {
  const files = document.getElementById("fileInput").files;
  const status = document.getElementById("status");

  if (!files.length) {
    status.textContent = "Please select at least one file.";
    status.style.color = "red";
    return;
  }

  status.textContent = "Uploading...";
  status.style.color = "black";

  for (const file of files) {
    const formData = new FormData();
    formData.append("file", file);
    try {
      const resp = await fetch("/Music/upload", {
        method: "POST",
        body: formData,
      });
      if (!resp.ok) throw new Error(await resp.text());
    } catch (err) {
      console.error(err);
      status.textContent = `Error: ${err.message}`;
      status.style.color = "red";
      return;
    }
  }

  status.textContent = "Upload complete.";
  status.style.color = "green";
  await fetchLibrary();
});

async function fetchLibrary() {
  const resp = await fetch("/Music/library");
  musicLibrary = await resp.json();
  renderAlbums();
}

function renderAlbums() {
  const albumList = document.getElementById("albumList");
  albumList.innerHTML = "";

  const grouped = groupBy(musicLibrary, "album");

  for (const [albumName, tracks] of Object.entries(grouped)) {
    const li = document.createElement("li");
    li.innerHTML = `
      <img src="${tracks[0].coverBase64 || 'img/placeholder1.jpg'}" />
      <span>${albumName}</span>
    `;
    li.onclick = () => loadAlbum(albumName);
    albumList.appendChild(li);
  }
}

function loadAlbum(albumName) {
  currentAlbum = musicLibrary.filter(t => t.album === albumName);
  currentIndex = 0;

  document.getElementById("album-title").textContent = albumName;
  renderTrackList();
  loadTrack(0);
}

function renderTrackList() {
  const trackList = document.getElementById("track-list");
  trackList.innerHTML = currentAlbum.map((t, i) => {
    const mins = Math.floor(t.duration / 60);
    const secs = Math.floor(t.duration % 60).toString().padStart(2, '0');
    return `<li data-index="${i}">
      ${t.title} — ${t.artist} <span class="duration">${mins}:${secs}</span>
    </li>`;
  }).join("");

  // click to jump
  document.querySelectorAll('#track-list li').forEach(li =>
    li.addEventListener('click', e => {
      const idx = +e.currentTarget.dataset.index;
      loadTrack(idx);
      playAudio();
    })
  );
}

function loadTrack(idx) {
  const t = currentAlbum[idx];
  if (!t) return;
  currentIndex = idx;

  // update player UI
  document.querySelector('.track-info img').src = t.coverBase64 || 'img/placeholder1.jpg';
  document.getElementById('song-title').textContent = t.title;
  document.getElementById('song-artist').textContent = t.artist;

  const player = document.getElementById('audioPlayer');
  player.src = t.path;
}

function togglePlay() {
  const player = document.getElementById('audioPlayer');
  const btn = document.getElementById('playBtn');
  if (player.paused) {
    playAudio();
  } else {
    player.pause();
    btn.textContent = '▶️';
  }
}

function playAudio() {
  const player = document.getElementById('audioPlayer');
  const btn = document.getElementById('playBtn');
  player.play();
  btn.textContent = '⏸️';
}

function prevTrack() {
  const prev = (currentIndex - 1 + currentAlbum.length) % currentAlbum.length;
  loadTrack(prev);
  playAudio();
}

function nextTrack() {
  const next = (currentIndex + 1) % currentAlbum.length;
  loadTrack(next);
  playAudio();
}

function setVolume(val) {
  document.getElementById('audioPlayer').volume = val / 100;
}

function groupBy(arr, key) {
  return arr.reduce((acc, obj) => {
    const k = obj[key] || 'Unknown';
    (acc[k] = acc[k] || []).push(obj);
    return acc;
  }, {});
}

// initial load
fetchLibrary();
