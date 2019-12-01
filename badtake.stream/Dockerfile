FROM node:13-alpine

RUN apk add --update nginx
RUN adduser -D -g 'www' www
RUN mkdir -p /var/www && chown -R www:www /var/lib/nginx && chown -R www:www /var/www

WORKDIR /usr/src/app

COPY package.json package-lock.json ./
RUN npm set progress=false && npm install

COPY . .
RUN npm run build
RUN mv -v ./build/* /var/www/

COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80

CMD nginx -g "daemon off;"