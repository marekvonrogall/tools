// js/app.js

let musicLibrary = [];
let currentAlbum = [];
let currentIndex = 0;

// Upload handling
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

// Fetch library metadata
async function fetchLibrary() {
  const resp = await fetch("/Music/library");
  musicLibrary = await resp.json();
  renderAlbums();
  clearTrackView();
}

// Sidebar albums
function renderAlbums() {
  const albumList = document.getElementById("albumList");
  albumList.innerHTML = "";
  const grouped = groupBy(musicLibrary, "album");
  for (const [albumName, tracks] of Object.entries(grouped)) {
    const li = document.createElement("li");
    li.innerHTML = `
      <img src="${tracks[0].coverUrl || "img/placeholder.png"}" alt="Cover" />
      <span>${albumName}</span>
    `;
    li.onclick = () => loadAlbum(albumName);
    albumList.appendChild(li);
  }
}

// Reset main view
function clearTrackView() {
  document.getElementById("album-title").textContent = "Tracks";
  document.getElementById("track-list").innerHTML = "";
  document.querySelector(".track-info img").src = "img/placeholder.png";
  document.getElementById("song-title").textContent = "";
  document.getElementById("song-artist").textContent = "";
  document.getElementById("audioPlayer").src = "";
  document.getElementById("playIcon").src = "img/play.svg";
}

// Load selected album
function loadAlbum(albumName) {
  currentAlbum = musicLibrary.filter((t) => t.album === albumName);
  currentIndex = 0;
  document.getElementById("album-title").textContent = albumName;
  renderTrackList();
}

// Track list
function renderTrackList() {
  const trackList = document.getElementById("track-list");
  trackList.innerHTML = "";

  currentAlbum.forEach((t, i) => {
    const mins = Math.floor(t.duration / 60);
    const secs = Math.floor(t.duration % 60)
      .toString()
      .padStart(2, "0");

    const li = document.createElement("li");
    li.dataset.index = i;

    li.innerHTML = `
        <span class="info">${t.title} — ${t.artist}</span>
        <span class="duration">${mins}:${secs}</span>
        <button class="delete-btn">
          <img src="img/recycle-bin.svg" alt="Delete" class="icon" />
        </button>
      `;

    // Attach delete button handler safely
    li.querySelector(".delete-btn").addEventListener("click", (e) => {
      e.stopPropagation(); // prevent triggering track load
      deleteTrack(t.fileName);
    });

    // Attach track load handler
    li.addEventListener("click", () => {
      loadTrack(i);
      playAudio();
    });

    trackList.appendChild(li);
  });
}

// Delete track
async function deleteTrack(encodedFileName) {
  const fileName = decodeURIComponent(encodedFileName);
  if (!confirm("Delete this track?")) return;
  const res = await fetch(`/Music/delete?fileName=${encodedFileName}`, {
    method: "DELETE",
  });
  if (!res.ok) return alert("Failed to delete.");
  musicLibrary = musicLibrary.filter((t) => t.fileName !== fileName);
  renderAlbums();
  const currentName = document.getElementById("album-title").textContent;
  const grouped = groupBy(musicLibrary, "album");
  if (!grouped[currentName]) {
    clearTrackView();
  } else {
    currentAlbum = grouped[currentName];
    renderTrackList();
  }
}

// stream track
function loadTrack(idx) {
  const t = currentAlbum[idx];
  if (!t) return;
  currentIndex = idx;
  document.querySelector(".track-info img").src =
    t.coverUrl || "img/placeholder.png";
  document.getElementById("song-title").textContent = t.title;
  document.getElementById("song-artist").textContent = t.artist;
  const streamUrl = `/Music/stream?fileName=${encodeURIComponent(t.fileName)}`;
  document.getElementById("audioPlayer").src = streamUrl;
}

// Playback controls
function togglePlay() {
  const player = document.getElementById("audioPlayer");
  const icon = document.getElementById("playIcon");
  if (player.paused) {
    player.play().catch(console.error);
    icon.src = "img/pause.svg";
  } else {
    player.pause();
    icon.src = "img/play.svg";
  }
}

function prevTrack() {
  if (!currentAlbum.length) return;
  const prev = (currentIndex - 1 + currentAlbum.length) % currentAlbum.length;
  loadTrack(prev);
  playAudio();
}

function nextTrack() {
  if (!currentAlbum.length) return;
  const next = (currentIndex + 1) % currentAlbum.length;
  loadTrack(next);
  playAudio();
}

function playAudio() {
  const player = document.getElementById("audioPlayer");
  const icon = document.getElementById("playIcon");
  player.play().catch(console.error);
  icon.src = "img/pause.svg";
}

function setVolume(val) {
  document.getElementById("audioPlayer").volume = val / 100;
}

function groupBy(arr, key) {
  return arr.reduce((acc, obj) => {
    const k = obj[key] || "Unknown";
    (acc[k] = acc[k] || []).push(obj);
    return acc;
  }, {});
}

// initial load
fetchLibrary();
