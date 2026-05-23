FROM node:24.14.0-alpine
WORKDIR /app
COPY package*.json ./
RUN npm install
COPY . .
EXPOSE 5085
CMD ["node", "server.js"]
