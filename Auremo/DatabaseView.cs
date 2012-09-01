﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Auremo
{
    public class DatabaseView
    {
        private Database m_Database = null;

        public delegate ISet<AlbumMetadata> AlbumsUnderRoot(string root);
        public delegate ISet<SongMetadata> SongsOnAlbum(AlbumMetadata album);

        #region Construction and setup

        public DatabaseView(Database database)
        {
            m_Database = database;

            Artists = new ObservableCollection<string>();
            AlbumsBySelectedArtists = new ObservableCollection<AlbumMetadata>();
            SongsOnSelectedAlbumsBySelectedArtists = new ObservableCollection<SongMetadata>();

            Genres = new ObservableCollection<string>();
            AlbumsOfSelectedGenres = new ObservableCollection<AlbumMetadata>();
            SongsOnSelectedAlbumsOfSelectedGenres = new ObservableCollection<SongMetadata>();

            ArtistTree = new ObservableCollection<TreeViewNode>();
            ArtistTreeController = new TreeViewController(ArtistTree);

            GenreTree = new ObservableCollection<TreeViewNode>();
            GenreTreeController = new TreeViewController(GenreTree);

            DirectoryTree = new ObservableCollection<TreeViewNode>();
            DirectoryTreeController = new TreeViewController(DirectoryTree);
        }

        public void Refresh()
        {
            PopulateArtists();
            AlbumsBySelectedArtists.Clear();
            SongsOnSelectedAlbumsBySelectedArtists.Clear();
            PopulateGenres();
            AlbumsOfSelectedGenres.Clear();
            SongsOnSelectedAlbumsOfSelectedGenres.Clear();
            PopulateDirectoryTree();
            PopulateArtistTree();
            PopulateGenreTree();
        }

        private void PopulateArtists()
        {
            Artists.Clear();

            foreach (string artist in m_Database.Artists)
            {
                Artists.Add(artist);
            }
        }

        private void PopulateGenres()
        {
            Genres.Clear();

            foreach (string genre in m_Database.Genres)
            {
                Genres.Add(genre);
            }
        }

        private void PopulateArtistTree()
        {
            ArtistTree.Clear();
            ArtistTreeController.MultiSelection.Clear();

            foreach (string artist in Artists)
            {
                ArtistTreeViewNode artistNode = new ArtistTreeViewNode(artist, null, ArtistTreeController);

                foreach (AlbumMetadata album in m_Database.AlbumsByArtist(artist))
                {
                    AlbumMetadataTreeViewNode albumNode = new AlbumMetadataTreeViewNode(album, artistNode, ArtistTreeController);
                    artistNode.AddChild(albumNode);

                    foreach (SongMetadata song in m_Database.SongsByAlbum(album))
                    {
                        SongMetadataTreeViewNode songNode = new SongMetadataTreeViewNode("", song, albumNode, ArtistTreeController);
                        albumNode.AddChild(songNode);
                    }
                }

                ArtistTree.Add(artistNode); // Insert now that branch is fully populated.
            }

            int id = 0;

            foreach (TreeViewNode baseNode in ArtistTree)
            {
                id = AssignTreeViewNodeIDs(baseNode, id);
            }
        }

        private void PopulateGenreTree()
        {
            GenreTreeController.ClearMultiSelection();
            GenreTree.Clear();

            foreach (string genre in Genres)
            {
                GenreTreeViewNode genreNode = new GenreTreeViewNode(genre, null, GenreTreeController);

                foreach (AlbumMetadata album in m_Database.AlbumsByGenre(genre))
                {
                    AlbumMetadataTreeViewNode albumNode = new AlbumMetadataTreeViewNode(album, genreNode, GenreTreeController);
                    genreNode.AddChild(albumNode);

                    foreach (SongMetadata song in m_Database.SongsByAlbum(album))
                    {
                        SongMetadataTreeViewNode songNode = new SongMetadataTreeViewNode("", song, albumNode, GenreTreeController);
                        albumNode.AddChild(songNode);
                    }
                }

                GenreTree.Add(genreNode);
            }

            int id = 0;

            foreach (TreeViewNode baseNode in GenreTree)
            {
                id = AssignTreeViewNodeIDs(baseNode, id);
            }
        }

        private void PopulateDirectoryTree()
        {
            DirectoryTreeController.ClearMultiSelection();
            DirectoryTree.Clear();

            DirectoryTreeViewNode rootNode = new DirectoryTreeViewNode("/", null, DirectoryTreeController);
            IDictionary<string, TreeViewNode> directoryLookup = new SortedDictionary<string, TreeViewNode>();
            directoryLookup[rootNode.DisplayString] = rootNode;

            foreach (SongMetadata song in m_Database.Songs)
            {
                Tuple<string, string> directoryAndFile = Utils.SplitPath(song.Path);
                TreeViewNode parent = FindDirectoryNode(directoryAndFile.Item1, directoryLookup, rootNode);
                SongMetadataTreeViewNode leaf = new SongMetadataTreeViewNode(directoryAndFile.Item2, song, parent, DirectoryTreeController);
                parent.AddChild(leaf);
            }

            AssignTreeViewNodeIDs(rootNode, 0);

            DirectoryTree.Add(rootNode);
            rootNode.IsExpanded = true;
        }

        private TreeViewNode FindDirectoryNode(string path, IDictionary<string, TreeViewNode> lookup, TreeViewNode rootNode)
        {
            if (path == "")
            {
                return rootNode;
            }
            else if (lookup.ContainsKey(path))
            {
                return lookup[path];
            }
            else
            {
                Tuple<string, string> parentAndSelf = Utils.SplitPath(path);
                TreeViewNode parent = FindDirectoryNode(parentAndSelf.Item1, lookup, rootNode);
                TreeViewNode self = new DirectoryTreeViewNode(parentAndSelf.Item2, parent, DirectoryTreeController);
                parent.AddChild(self);
                lookup[path] = self;
                return self;
            }
        }

        private int AssignTreeViewNodeIDs(TreeViewNode node, int nodeID)
        {
            node.ID = nodeID;
            int nextNodeID = nodeID + 1;

            foreach (TreeViewNode child in node.Children)
            {
                nextNodeID = AssignTreeViewNodeIDs(child, nextNodeID);
            }

            return nextNodeID;
        }

        #endregion

        #region Artist/album/song view

        public IList<string> Artists
        {
            get;
            private set;
        }

        public IList<AlbumMetadata> AlbumsBySelectedArtists
        {
            get;
            private set;
        }

        public IList<SongMetadata> SongsOnSelectedAlbumsBySelectedArtists
        {
            get;
            private set;
        }

        public void OnSelectedArtistsChanged(IList selection)
        {
            OnRootLevelSelectionChanged(selection, AlbumsBySelectedArtists, m_Database.AlbumsByArtist);
        }

        public void OnSelectedAlbumsBySelectedArtistsChanged(IList selection)
        {
            OnAlbumLevelSelectionChanged(selection, SongsOnSelectedAlbumsBySelectedArtists, m_Database.SongsByAlbum);
        }

        #endregion

        #region Genre/album/artist view

        public IList<string> Genres
        {
            get;
            private set;
        }

        public IList<AlbumMetadata> AlbumsOfSelectedGenres
        {
            get;
            private set;
        }

        public IList<SongMetadata> SongsOnSelectedAlbumsOfSelectedGenres
        {
            get;
            private set;
        }

        public void OnSelectedGenresChanged(IList selection)
        {
            OnRootLevelSelectionChanged(selection, AlbumsOfSelectedGenres, m_Database.AlbumsByGenre);
        }

        public void OnSelectedAlbumsOfSelectedGenresChanged(IList selection)
        {
            OnAlbumLevelSelectionChanged(selection, SongsOnSelectedAlbumsOfSelectedGenres, m_Database.SongsByAlbum);
        }

        #endregion

        #region Artist/album/song tree view

        public IList<TreeViewNode> ArtistTree
        {
            get;
            private set;
        }

        public TreeViewController ArtistTreeController
        {
            get;
            private set;
        }

        public ISet<SongMetadataTreeViewNode> ArtistTreeSelectedSongs
        {
            get
            {
                return ArtistTreeController.Songs;
            }
        }

        #endregion

        #region Genre/album/song tree view

        public IList<TreeViewNode> GenreTree
        {
            get;
            private set;
        }

        public TreeViewController GenreTreeController
        {
            get;
            private set;
        }

        public ISet<SongMetadataTreeViewNode> GenreTreeSelectedSongs
        {
            get
            {
                return GenreTreeController.Songs;
            }
        }

        #endregion

        #region Directory tree view

        public IList<TreeViewNode> DirectoryTree
        {
            get;
            private set;
        }

        public TreeViewController DirectoryTreeController
        {
            get;
            private set;
        }

        public ISet<SongMetadataTreeViewNode> DirectoryTreeSelectedSongs
        {
            get
            {
                return DirectoryTreeController.Songs;
            }
        }

        #endregion

        #region Helpers

        private void OnRootLevelSelectionChanged(IList newSelection, IList<AlbumMetadata> albumView, AlbumsUnderRoot Albums)
        {
            albumView.Clear();
            ISet<string> sortedItems = new SortedSet<string>();

            foreach (object o in newSelection)
            {
                sortedItems.Add(o as string);
            }

            foreach (string item in sortedItems)
            {
                foreach (AlbumMetadata album in Albums(item))
                {
                    albumView.Add(album);
                }
            }
        }

        private void OnAlbumLevelSelectionChanged(IList newSelection, IList<SongMetadata> songView, SongsOnAlbum Songs)
        {
            songView.Clear();
            ISet<AlbumMetadata> sortedAlbums = new SortedSet<AlbumMetadata>();

            foreach (object o in newSelection)
            {
                sortedAlbums.Add(o as AlbumMetadata);
            }

            foreach (AlbumMetadata album in sortedAlbums)
            {
                foreach (SongMetadata song in Songs(album))
                {
                    songView.Add(song);
                }
            }
        }

        #endregion
    }
}
