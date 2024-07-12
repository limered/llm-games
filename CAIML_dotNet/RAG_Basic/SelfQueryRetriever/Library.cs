using LangChain.DocumentLoaders;

namespace SelfQueryRetriever;

public static class Library
{
    public static Document[] Books()
    {
        return
        [
            new Document
            {
                PageContent =
                    "Portrays Elizabeth Bennet's growth in discerning true character over appearances, set against Regency England's social mores.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "Pride and Prejudice" },
                    { "author", "Jane Austen" },
                    { "first_published", 1813 },
                    { "genre", "Romance" },
                    { "origin", "England" },
                    { "rating", 4.5 }
                }
            },
            new Document
            {
                PageContent =
                    "Resists a totalitarian regime under constant surveillance, showcasing the perils of absolute power and the spirit of rebellion.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "1984" },
                    { "author", "George Orwell" },
                    { "first_published", 1949 },
                    { "genre", "Dystopian" },
                    { "origin", "England" },
                    { "rating", 4.7 }
                }
            },
            new Document
            {
                PageContent =
                    "Exposes racial injustices in the South through Scout Finch, whose father defends a wrongly accused black man, challenging societal prejudices.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "To Kill a Mockingbird" },
                    { "author", "Harper Lee" },
                    { "first_published", 1960 },
                    { "genre", "Southern Gothic" },
                    { "origin", "United States" },
                    { "rating", 4.8 }
                }
            },
            new Document
            {
                PageContent =
                    "Reveals the Jazz Age's allure and despair through Gatsby's doomed love, critiquing the American Dream's corruption.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "The Great Gatsby" },
                    { "author", "F. Scott Fitzgerald" },
                    { "first_published", 1925 },
                    { "genre", "Tragedy" },
                    { "origin", "United States" },
                    { "rating", 4.6 }
                }
            },
            new Document
            {
                PageContent =
                    "Follows Frodo Baggins on a quest to destroy a powerful ring, weaving a tale of bravery, friendship, and darkness in Middle-earth.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "The Lord of the Rings" },
                    { "author", "J.R.R. Tolkien" },
                    { "first_published", 1954 },
                    { "genre", "Fantasy" },
                    { "origin", "England" },
                    { "rating", 4.9 }
                }
            },
            new Document
            {
                PageContent =
                    "Confronts the haunting legacies of slavery through Sethe, a former slave tormented by her past, exploring themes of family and freedom.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "Beloved" },
                    { "author", "Toni Morrison" },
                    { "first_published", 1987 },
                    { "genre", "Historical Fiction" },
                    { "origin", "United States" },
                    { "rating", 4.7 }
                }
            },
            new Document
            {
                PageContent =
                    "Captures teenage angst and alienation through Holden Caulfield's cynical view of adult hypocrisy and the pains of growing up.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "The Catcher in the Rye" },
                    { "author", "J.D. Salinger" },
                    { "first_published", 1951 },
                    { "genre", "Coming-of-Age" },
                    { "origin", "United States" },
                    { "rating", 4.5 }
                }
            },
            new Document
            {
                PageContent =
                    "Examines the fallout of unchecked scientific ambition through Victor Frankenstein's creation of life, highlighting the ethical limits of science.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "Frankenstein" },
                    { "author", "Mary Shelley" },
                    { "first_published", 1818 },
                    { "genre", "Science Fiction" },
                    { "origin", "England" },
                    { "rating", 4.6 }
                }
            },
            new Document
            {
                PageContent =
                    "Depicts a future where technological progress has stunted humanity, questioning the cost of happiness and freedom.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "Brave New World" },
                    { "author", "Aldous Huxley" },
                    { "first_published", 1932 },
                    { "genre", "Science Fiction" },
                    { "origin", "England" },
                    { "rating", 4.7 }
                }
            },
            new Document
            {
                PageContent =
                    "Takes Arthur Dent on a ludicrous space journey, poking fun at life's absurdities and the universe's vast mysteries and the number 42.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "The Hitchhiker's Guide to the Galaxy" },
                    { "author", "Douglas Adams" },
                    { "first_published", 1979 },
                    { "genre", "Science Fiction" },
                    { "origin", "England" },
                    { "rating", 4.5 }
                }
            },
            new Document
            {
                PageContent =
                    "Delves into Raskolnikov's psyche after he murders for a 'noble' cause, probing the depths of guilt, morality, and redemption in bleak Russia.",
                Metadata = new Dictionary<string, object>
                {
                    { "name", "Crime and Punishment" },
                    { "author", "Fyodor Dostoevsky" },
                    { "first_published", 1866 },
                    { "genre", "Psychological Fiction" },
                    { "origin", "Russia" },
                    { "rating", 4.3 }
                }
            }
        ];
    }
}